# -*- coding: utf-8 -*-
"""
Created on Wed Jan 25 20:46:00 2023

@author: mrsag
"""

import numpy as np
import matplotlib.pyplot as plt
from scipy.integrate import quad
import sys
import os

import matplotlib
matplotlib.rcParams['figure.dpi'] = 500

# Sample and Sum Length from command-line arguments
terms = int(sys.argv[1])  # Get from command line
# sum_length = int(sys.argv[2])

# Path for saving the output image in the script's directory
script_dir = os.path.dirname(os.path.realpath(__file__))
output_image_path = os.path.join(script_dir, "output.png")


# Define the function for which you want to calculate Fourier coefficients
def g(x):
    # return((x**3*(abs(x)<2)+np.log(abs(x)+1)))
    #return(np.sinc(x))
    return (abs(x)<np.pi/2)

@np.vectorize
def f(x):
    if abs(x)>np.pi:
        if(x>0):
            return(f(x-2*np.pi))
        else:
            return(f(x+2*np.pi))
        
    else:
        return g(x)

# Define the range of integration
a = -np.pi
b = np.pi

# Define the number of terms in the Fourier series
n_terms = int(sys.argv[1])


##########################
f_type = 'unknown'
def is_even(f, domain):
    for x in domain:
        if f(x) != f(-x):
            return False
    return True


def is_odd(f, domain):
    for x in domain:
        if f(x) != (-1)*f(-x):
            return False
    return True
#########################
domain = np.linspace(-np.pi,np.pi,101)
if(is_even(f, domain)):
    f_type = 'even'
if(is_odd(f, domain)):
    f_type = 'odd'
if not (is_even(f, domain)) and not (is_odd(f, domain)):
    f_type = 'mixed'
    
print(f"f_type: {f_type}")

# Function to calculate the Fourier coefficients for a given function and integer n
def fourier_coefficients(n):
    # Define the integrands for Fourier coefficients a_n and b_n
    def integrand_cos(x):
        return f(x) * np.cos(n * x)

    def integrand_sin(x):
        return f(x) * np.sin(n * x)
    
    if(f_type =='even'):
        a_n, _ = quad(integrand_cos, a, b)
        b_n = 0
    if(f_type == 'odd'):
        a_n = 0
        b_n, _ = quad(integrand_sin, a, b)
    if(f_type == 'mixed'):
        # Perform numerical integration using quad
        a_n, _ = quad(integrand_cos, a, b)
        b_n, _ = quad(integrand_sin, a, b)

    # Normalize coefficients
    a_n *= 1 / np.pi
    b_n *= 1 / np.pi

    return a_n, b_n

# Calculate Fourier coefficients for n_terms
coefficients = [fourier_coefficients(n) for n in range(0, n_terms + 1)]

# Print the Fourier coefficients
for n, (a_n, b_n) in enumerate(coefficients, 0):
    print(f"a_{n}: {a_n*(a_n>1e-5):.5f}, b_{n}: {b_n*(b_n>1e-5):.5f}")
    
    
def fourier_calculator(x,coeff):
    y = coeff[0][0]/2
    for i in range(1,len(coeff)):
        y += coeff[i][0]*np.cos(i*x)+coeff[i][1]*np.sin(i*x)
        
    return(y)

x = np.linspace(3*a,3*b,1000)
y = f(x)
y_predict = fourier_calculator(x, coefficients)


fig, ax = plt.subplots()
plt.subplots_adjust(left=0.1, bottom=0.25)
line1, = ax.plot(x, y,'go-', label="Given function", markersize=1)
line, = ax.plot(x, y_predict, 'r-', lw=0.5)
ax.set_xlabel('x')
ax.set_ylabel('y')
title = ax.set_title(f"degree: {n_terms}")


# Save the plot in the same directory as the script
plt.savefig(output_image_path,dpi=500)