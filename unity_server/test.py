from uarm_gym_env import CustomUnityEnv

from time import sleep, time
from random import choice

c = CustomUnityEnv()

c.reset()
for i in range(10):
    c.step(0)
    sleep(1)

c._kill_unity()
