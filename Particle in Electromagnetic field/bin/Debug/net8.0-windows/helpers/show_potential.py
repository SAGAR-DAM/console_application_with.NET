import numpy as np
import json
from mayavi import mlab
import glob
import matplotlib.pyplot as plt

with open('config.json', 'r') as f:
    data = json.load(f)


nx = data["nx"]
ny = data["ny"]
nz = data["nz"]
lx = data["Lx"]*1e-2
ly = data["Ly"]*1e-2
lz = data["Lz"]*1e-2

del data
data = np.loadtxt("potential.txt")

x, y, z = np.mgrid[0:lx:nx*1j,0:ly:ny*1j,0:lz:nz*1j]

potential = data[:]
potential=potential.reshape(x.shape)

contours=50
opacity=0.08
cmap="jet"
mlab.figure(bgcolor=(1, 1, 1), size=(800, 600))
mlab.contour3d(x, y, z, potential, contours=contours, opacity=opacity, colormap=cmap)
mlab.colorbar(title="potential", orientation='vertical')
axes = mlab.axes(xlabel='X', ylabel='Y', zlabel='Z',color=(1.0,0.0,0.0))
axes.title_text_property.color = (1.0, 0.0, 0.0)
axes.label_text_property.color = (1.0, 0.0, 0.0)
mlab.title("3D Isosurface of Potential")
mlab.show()