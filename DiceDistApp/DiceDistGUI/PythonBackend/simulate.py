import numpy as np
import matplotlib.pyplot as plt
from scipy.stats import norm
import sys
import os

import matplotlib
matplotlib.rcParams['figure.dpi'] = 500

# Get the directory where the script is located
script_dir = os.path.dirname(os.path.realpath(__file__))

# Sample and Sum Length from command-line arguments
sample = int(sys.argv[1])  # Get from command line
sum_length = int(sys.argv[2])

# Path for saving the output image in the script's directory
output_image_path = os.path.join(script_dir, "output.png")

dist = []
for i in range(sample):
    x = np.random.randint(low=1, high=7, size=sum_length)
    dist.append(np.mean(x))

dist = np.array(dist)
dist = (dist - np.mean(dist)) / np.std(dist)

plt.hist(dist, bins=min([50,int(sample // 20)]), density=True)
x = np.linspace(-4, 4, 1000)
plt.plot(x, norm.pdf(x, 0, 1), 'r-', label='Standard Normal $N(0,1)$')
plt.title(f"Sample={sample}, Sum Length={sum_length}")
plt.legend()

# Save the plot in the same directory as the script
plt.savefig(output_image_path)
