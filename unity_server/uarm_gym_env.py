import subprocess
import os
from pathlib import Path

import cv2
import numpy as np
from gym import spaces

from context_timer import ContextTimer
from server import UnityInterface
from subprocess import check_output


def get_pid(name):
    try:
        return str(check_output(["pidof", "-s", name]))[2:-3]
    except:
        return False


class CustomUnityEnv():
    GAMES_BETWEEN_RESTARTS = 50
    EXECUTABLE_PATH = "/home/adryw/Documents/DRL_Playground/Build/test_build.x86_64"

    observation_space = spaces.Box(0, 0, shape=(84, 84, 1))
    action_space = spaces.Discrete(n=6)

    def __init__(self):
        self.steps_since_restart = 0
        self.total_steps_ever = 0
        self.latest_total_score = 0
        self.total_games = 0
        self.episode_rewards = []

        # Make sure there aren't any other unity processes running
        self._kill_unity()

        # Run server
        self._open_unity()
        self.server = UnityInterface("localhost", 1234)

    def step(self, action):
        self.steps_since_restart += 1
        self.total_steps_ever += 1

        # Send a state and get a response
        with ContextTimer(post_print=False) as timer:
            self.server.send_state(action)
            is_over, image, new_score = self._get_state()

        # # Print FPS?
        # if self.total_steps_ever % 1000 == 0:
        #     print("FPS", 1 / (timer.elapsed + .00001))

        # Update the score and log info
        reward = new_score - self.latest_total_score
        self.latest_total_score = new_score
        return image, reward, is_over, None

    def reset(self):
        """Return the first observation after reset"""
        self.total_games += 1
        # Force a restart of the game in case of glitched-out robot
        if self.total_games % self.GAMES_BETWEEN_RESTARTS == 0:
            self.server.disconnect()
            self._kill_unity()
            self._open_unity()
            self.server.connect()

        # Record the previous episodes rewards
        self.episode_rewards.append(self.latest_total_score)

        # Reset the game
        self.server.send_reset()
        is_over, image, new_score = self._get_state()
        self.latest_total_score = new_score
        return image

    def close(self):
        """Called after all training is done"""
        pass

    def _get_state(self):
        """"""
        is_over, image, new_score = self.server.get_state()
        image_gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
        image_gray = cv2.resize(image_gray, (84, 84))
        image_gray = np.expand_dims(image_gray, axis=2)
        return is_over, image_gray, new_score

    def _open_unity(self):
        print("Running Unity process")
        subprocess.Popen([self.EXECUTABLE_PATH])

    def _kill_unity(self):
        process_name = Path(self.EXECUTABLE_PATH).name
        print("Killing Unity process", process_name)
        # os.system("taskkill /f /im " + process_name)
        id = get_pid(process_name)
        if id:
            os.system("kill " + id)
