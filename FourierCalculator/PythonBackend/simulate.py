import sys
import os
from sympy import (
    sympify, symbols, lambdify,
    sin, cos, tan, csc, sec, cot,
    asin, acos, atan, acsc, asec, acot,
    log, exp, pi
)
import numpy as np
from scipy.integrate import quad
import matplotlib.pyplot as plt

# Read command-line arguments
expr_str = sys.argv[1]       # e.g. "sin(1+(log(x))**2)"
a, b      = float(sys.argv[2]), float(sys.argv[3])
n_terms   = int(sys.argv[4])

# Define symbol
x = symbols('x')

# Allow only safe functions
allowed = {
    'sin': sin,   'cos': cos,   'tan': tan,
    'csc': csc,   'sec': sec,   'cot': cot,
    'asin': asin, 'acos': acos, 'atan': atan,
    'acsc': acsc, 'asec': asec, 'acot': acot,
    'log': log,   'exp': exp,   'pi': pi
}

# Parse expression safely
try:
    sym_expr = sympify(expr_str, locals=allowed)
except Exception as e:
    print(f"Error parsing expression: {e}", file=sys.stderr)
    sys.exit(1)

# Convert symbolic expression to numeric function
def f_num(x_val):
    # ensure periodic extension within [a, b]
    # map x_val into [a, b]
    period = b - a
    t = ((x_val - a) % period) + a
    return lambdify(x, sym_expr, modules=['numpy'])(t)

# Sample grid for even/odd detection
if(b!=-a):
    f_type = "mixed"
if(b==-a):
    test_x = np.linspace(a, b, 201)
    test_vals = f_num(test_x)
    if np.allclose(test_vals, f_num(-test_x), atol=1e-2):
        f_type = 'even'
    if np.allclose(-test_vals, f_num(-test_x), atol=1e-2):
        f_type = 'odd'
    else:
        f_type = 'mixed'
    # print(f"f(x)+f(-x): {np.sum(np.abs(f_num(-test_x)+test_vals))}")
print(f"Detected function type: {f_type}")

# Compute Fourier coefficients
def coeffs(n):
    if f_type in ('even', 'mixed'):
        a_n, _ = quad(lambda t: f_num(t) * np.cos(n*t*2*np.pi/(b-a)), a, b)
    else:
        a_n = 0.0
    if f_type in ('odd', 'mixed'):
        b_n, _ = quad(lambda t: f_num(t) * np.sin(n*t*2*np.pi/(b-a)), a, b)
    else:
        b_n = 0.0
    return 2* a_n / (b-a), 2* b_n / (b-a)

# Calculate coefficients up to n_terms
coeff_list = [coeffs(n) for n in range(n_terms + 1)]
for n, (an, bn) in enumerate(coeff_list):
    print(f"a_{n} = {an:.5f}, b_{n} = {bn:.5f}")

# Reconstruct and plot
xx_full = np.linspace(3*a, 3*b, 1000)
def fourier_series(x_val):
    result = coeff_list[0][0] / 2.0
    for k in range(1, len(coeff_list)):
        an, bn = coeff_list[k]
        result += an * np.cos(2 * np.pi * k * x_val/(b-a)) + bn * np.sin(2 * np.pi * k * x_val/(b-a))
    return result

yy = f_num(xx_full)
yy_est = fourier_series(xx_full)

plt.figure(dpi=300)
plt.plot(xx_full, yy, 'g-', label='f(x)')
plt.plot(xx_full, yy_est, 'r-', label='Fourier approx', lw=1)
plt.xlabel('x')
plt.ylabel('y')
plt.title(f'Fourier Series Approx. (n={n_terms})')
plt.legend()

# Save output image next to script
script_dir = os.path.dirname(os.path.realpath(__file__))
output_image_path = os.path.join(script_dir, 'output.png')
plt.savefig(output_image_path, dpi=300)
