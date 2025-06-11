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
dx = np.diff(np.linspace(0,lx,nx))[0]
dy = np.diff(np.linspace(0,ly,ny))[0]
dz = np.diff(np.linspace(0,lz,nz))[0]

potential = data[:]
potential=potential.reshape(x.shape)

Ex, Ey, Ez = np.gradient(-potential, dx, dy, dz, edge_order=2)
stepx = 1
stepy = 1
stepz = 1

x,y,z = x[::stepx,::stepy,::stepz], y[::stepx,::stepy,::stepz], z[::stepx,::stepy,::stepz]
Ex,Ey,Ez = Ex[::stepx,::stepy,::stepz], Ey[::stepx,::stepy,::stepz], Ez[::stepx,::stepy,::stepz]

mlab.figure(bgcolor=(1,1,1))
mlab.quiver3d(x, y, z, Ex, Ey, Ez,
            mode='arrow',
            colormap='jet')
axes = mlab.axes(xlabel='X', ylabel='Y', zlabel='Z',color=(1.0,0.0,0.0))
axes.title_text_property.color = (1.0, 0.0, 0.0)  # red title text (if titles used)
axes.label_text_property.color = (1.0, 0.0, 0.0)  # blue label text
mlab.title('3D Electric Field')
mlab.show()