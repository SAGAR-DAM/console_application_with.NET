/*
Author: SAGAR DAM
Date: 25.05.2025

This is the basic code for a particle propagator in the arbitrarily given E and B field.
Generally in experiemtal situation, the B filed is fixed and given by some permanent magnet.

The E field is defined by the potentials on several electrodes in the system. Here in this system we apply steady
B field (Bx, By, Bz) as the global variable. Then in the main funciton we define the simulation box as a 3d box with
given sides and resolution. The electrodeds can take the shapes like plate (capacitor), sphere, cylinder, hyperboloide,
ellipsoid, hollow pipe, or rectangular box. And combination of them that gives a valid boundary condition is also allowed.
Arbitrary shapes are not possible in this scope.

The simulator solves the Laplace equation numerically over the grid points and. The test particle (or multiparticles) can
be injected into the system with initial position and velocity. The function named propagator(particle) uses boris pusher
to simulate the particle trajectory over given time and resolution. The required variables to plot is then passed
to a temporary python __main__ for easier visualization. The python package Mayavi is used for the visual tool.

*/

#include <iomanip>
#include <fstream>
#include <limits>
#include <vector>
#include <array>
#include <cmath>
#include <string>
#include <iostream>
#include <algorithm>
#include <filesystem>
#include "json.hpp"
// #include "C:\\Users\\mrsag\\AppData\\Local\\Programs\\Python\\Python311\\include\\Python.h"

using namespace std;
using json = nlohmann::json;
namespace fs = std::filesystem;


double Bx, By, Bz;

double cm = 1e-2;
double mm = 1e-3;
double qe = 1.60217663e-19;
double me = 9.10938e-31;
double mH = 1.67262192e-27;
double c = 3e8;

double kev_to_joule = 1.660217663e-16;

/*
//////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////

                                3D SIMULATION BOX OBJECT FOR MAIN GEOMETRY

//////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////
*/

class SimulationBox3D
{
public:
    int nx, ny, nz;
    double lx, ly, lz;
    double dx, dy, dz;

    SimulationBox3D(int nx, int ny, int nz,
                    double lx, double ly, double lz,
                    double potential_offset = 0.0);

    void addSphere(double cx, double cy, double cz, double radius, double potential_value);
    void addBox(double x0, double y0, double z0,
                double x1, double y1, double z1, double potential_value);

    void addCylinder(double cx, double cy, double cz, double radius, double height, char axis, double potential_value);
    void addHollowPipe(double cx, double cy, double cz, double radius, double thickness, double height, char axis, double potential_value);
    void addEllipsoid(double cx, double cy, double cz, double rx, double ry, double rz, double potential_value);
    void addHyperboloid(double cx, double cy, double cz, double a, double b, double c, double waist, char axis, double potential_value);
    void addPlane(double A, double B, double C, double D, double thickness, double potential_value);

    void solve(int max_iter = 1000, double tol = 1e-4, const std::string &method = "jacobi");

    // Add geometry will be added next (e.g., addBox, addSphere)

    const std::vector<double> &getPotential() const { return potential; }

    std::vector<double> potential;
    std::vector<double> geometry;
    std::vector<bool> fixed_mask;

    int index(int i, int j, int k) const
    {
        return i * ny * nz + j * nz + k;
    }

    void applyJacobi();
    void applyGaussSeidel();
};

SimulationBox3D::SimulationBox3D(int nx, int ny, int nz,
                                 double lx, double ly, double lz,
                                 double potential_offset)
    : nx(nx), ny(ny), nz(nz), lx(lx), ly(ly), lz(lz)
{
    dx = lx / (nx - 1);
    dy = ly / (ny - 1);
    dz = lz / (nz - 1);

    int total_size = nx * ny * nz;
    potential.resize(total_size, potential_offset);
    geometry = potential;
    fixed_mask.resize(total_size, false);
}

void SimulationBox3D::solve(int max_iter, double tol, const std::string &method)
{
    std::vector<double> V_old = potential;
    std::vector<double> V_new = potential;

    for (int iter = 0; iter < max_iter; ++iter)
    {
        V_old = potential;

        if (method == "jacobi")
        {
            applyJacobi();
        }
        else if (method == "gauss-seidel")
        {
            applyGaussSeidel();
        }
        else
        {
            throw std::runtime_error("Unknown method");
        }

        // Check for convergence
        double max_diff = 0.0;
        for (size_t i = 0; i < potential.size(); ++i)
        {
            max_diff = std::max(max_diff, std::abs(potential[i] - V_old[i]));
        }

        std::cout << "Iteration " << iter << ", Max Diff = " << max_diff << std::endl;

        if (max_diff <= tol)
        {
            std::cout << "Converged at iteration " << iter << std::endl;
            break;
        }
    }
}

void SimulationBox3D::addSphere(double cx, double cy, double cz, double radius, double potential_value)
{
    for (int i = 0; i < nx; ++i)
    {
        double x = i * dx;
        for (int j = 0; j < ny; ++j)
        {
            double y = j * dy;
            for (int k = 0; k < nz; ++k)
            {
                double z = k * dz;
                double dist = std::sqrt((x - cx) * (x - cx) +
                                        (y - cy) * (y - cy) +
                                        (z - cz) * (z - cz));
                if (dist <= radius)
                {
                    int idx = index(i, j, k);
                    potential[idx] = potential_value;
                    geometry[idx] = potential_value;
                    fixed_mask[idx] = true;
                }
            }
        }
    }
}

void SimulationBox3D::addBox(double x0, double y0, double z0,
                             double x1, double y1, double z1, double potential_value)
{
    for (int i = 0; i < nx; ++i)
    {
        double x = i * dx;
        if (x < x0 || x > x1)
            continue;
        for (int j = 0; j < ny; ++j)
        {
            double y = j * dy;
            if (y < y0 || y > y1)
                continue;
            for (int k = 0; k < nz; ++k)
            {
                double z = k * dz;
                if (z < z0 || z > z1)
                    continue;

                int idx = index(i, j, k);
                potential[idx] = potential_value;
                geometry[idx] = potential_value;
                fixed_mask[idx] = true;
            }
        }
    }
}

void SimulationBox3D::addCylinder(double cx, double cy, double cz, double radius, double height, char axis, double potential_value)
{
    for (int i = 0; i < nx; ++i)
    {
        double x = i * dx;
        for (int j = 0; j < ny; ++j)
        {
            double y = j * dy;
            for (int k = 0; k < nz; ++k)
            {
                double z = k * dz;

                bool inside = false;
                if (axis == 'z')
                {
                    double r2 = (x - cx) * (x - cx) + (y - cy) * (y - cy);
                    inside = (r2 <= radius * radius && z >= cz && z <= cz + height);
                }
                else if (axis == 'x')
                {
                    double r2 = (y - cy) * (y - cy) + (z - cz) * (z - cz);
                    inside = (r2 <= radius * radius && x >= cx && x <= cx + height);
                }
                else if (axis == 'y')
                {
                    double r2 = (x - cx) * (x - cx) + (z - cz) * (z - cz);
                    inside = (r2 <= radius * radius && y >= cy && y <= cy + height);
                }

                if (inside)
                {
                    int idx = index(i, j, k);
                    potential[idx] = potential_value;
                    geometry[idx] = potential_value;
                    fixed_mask[idx] = true;
                }
            }
        }
    }
}

void SimulationBox3D::addHollowPipe(double cx, double cy, double cz, double radius, double thickness, double height, char axis, double potential_value)
{
    double r_outer2 = (radius + thickness / 2.0) * (radius + thickness / 2.0);
    double r_inner2 = (radius - thickness / 2.0) * (radius - thickness / 2.0);

    for (int i = 0; i < nx; ++i)
    {
        double x = i * dx;
        for (int j = 0; j < ny; ++j)
        {
            double y = j * dy;
            for (int k = 0; k < nz; ++k)
            {
                double z = k * dz;

                bool inside = false;
                if (axis == 'z')
                {
                    double r2 = (x - cx) * (x - cx) + (y - cy) * (y - cy);
                    inside = (r2 >= r_inner2 && r2 <= r_outer2 && z >= cz && z <= cz + height);
                }
                else if (axis == 'x')
                {
                    double r2 = (y - cy) * (y - cy) + (z - cz) * (z - cz);
                    inside = (r2 >= r_inner2 && r2 <= r_outer2 && x >= cx && x <= cx + height);
                }
                else if (axis == 'y')
                {
                    double r2 = (x - cx) * (x - cx) + (z - cz) * (z - cz);
                    inside = (r2 >= r_inner2 && r2 <= r_outer2 && y >= cy && y <= cy + height);
                }

                if (inside)
                {
                    int idx = index(i, j, k);
                    potential[idx] = potential_value;
                    geometry[idx] = potential_value;
                    fixed_mask[idx] = true;
                }
            }
        }
    }
}

void SimulationBox3D::addEllipsoid(double cx, double cy, double cz, double rx, double ry, double rz, double potential_value)
{
    for (int i = 0; i < nx; ++i)
    {
        double x = i * dx;
        for (int j = 0; j < ny; ++j)
        {
            double y = j * dy;
            for (int k = 0; k < nz; ++k)
            {
                double z = k * dz;
                double value = ((x - cx) / rx) * ((x - cx) / rx) + ((y - cy) / ry) * ((y - cy) / ry) + ((z - cz) / rz) * ((z - cz) / rz);
                if (value <= 1.0)
                {
                    int idx = index(i, j, k);
                    potential[idx] = potential_value;
                    geometry[idx] = potential_value;
                    fixed_mask[idx] = true;
                }
            }
        }
    }
}

void SimulationBox3D::addHyperboloid(double cx, double cy, double cz, double a, double b, double c, double waist, char axis, double potential_value)
{
    for (int i = 0; i < nx; ++i)
    {
        double x = i * dx;
        for (int j = 0; j < ny; ++j)
        {
            double y = j * dy;
            for (int k = 0; k < nz; ++k)
            {
                double z = k * dz;
                double val = 0.0;
                if (axis == 'x')
                {
                    val = -((x - cx) / a) * ((x - cx) / a) + ((y - cy) / b) * ((y - cy) / b) + ((z - cz) / c) * ((z - cz) / c);
                }
                else if (axis == 'y')
                {
                    val = ((x - cx) / a) * ((x - cx) / a) - ((y - cy) / b) * ((y - cy) / b) + ((z - cz) / c) * ((z - cz) / c);
                }
                else if (axis == 'z')
                {
                    val = ((x - cx) / a) * ((x - cx) / a) + ((y - cy) / b) * ((y - cy) / b) - ((z - cz) / c) * ((z - cz) / c);
                }

                if (val <= waist * waist)
                {
                    int idx = index(i, j, k);
                    potential[idx] = potential_value;
                    geometry[idx] = potential_value;
                    fixed_mask[idx] = true;
                }
            }
        }
    }
}

void SimulationBox3D::addPlane(double A, double B, double C, double D, double thickness, double potential_value)
{
    double norm = std::sqrt(A * A + B * B + C * C);
    for (int i = 0; i < nx; ++i)
    {
        double x = i * dx;
        for (int j = 0; j < ny; ++j)
        {
            double y = j * dy;
            for (int k = 0; k < nz; ++k)
            {
                double z = k * dz;
                double dist = (A * x + B * y + C * z + D) / norm;
                if (std::abs(dist) <= thickness / 2.0)
                {
                    int idx = index(i, j, k);
                    potential[idx] = potential_value;
                    geometry[idx] = potential_value;
                    fixed_mask[idx] = true;
                }
            }
        }
    }
}

void SimulationBox3D::applyJacobi()
{
    std::vector<double> new_potential = potential;

    // Interior points
    for (int i = 1; i < nx - 1; ++i)
    {
        for (int j = 1; j < ny - 1; ++j)
        {
            for (int k = 1; k < nz - 1; ++k)
            {
                int idx = index(i, j, k);
                if (!fixed_mask[idx])
                {
                    new_potential[idx] = (1.0 / 6.0) * (potential[index(i + 1, j, k)] + potential[index(i - 1, j, k)] +
                                                        potential[index(i, j + 1, k)] + potential[index(i, j - 1, k)] +
                                                        potential[index(i, j, k + 1)] + potential[index(i, j, k - 1)]);
                }
            }
        }
    }

    // Face points
    for (int j = 1; j < ny - 1; ++j)
    {
        for (int k = 1; k < nz - 1; ++k)
        {
            int idx = index(0, j, k);
            if (!fixed_mask[idx])
                new_potential[idx] = (1.0 / 5.0) * (potential[index(1, j, k)] + potential[index(0, j + 1, k)] +
                                                    potential[index(0, j - 1, k)] + potential[index(0, j, k + 1)] +
                                                    potential[index(0, j, k - 1)]);

            int idx1 = index(nx - 1, j, k);
            if (!fixed_mask[idx1])
                new_potential[idx1] = (1.0 / 5.0) * (potential[index(nx - 2, j, k)] + potential[index(nx - 1, j + 1, k)] +
                                                     potential[index(nx - 1, j - 1, k)] + potential[index(nx - 1, j, k + 1)] +
                                                     potential[index(nx - 1, j, k - 1)]);
        }
    }

    for (int i = 1; i < nx - 1; ++i)
    {
        for (int k = 1; k < nz - 1; ++k)
        {
            int idx = index(i, 0, k);
            if (!fixed_mask[idx])
                new_potential[idx] = (1.0 / 5.0) * (potential[index(i + 1, 0, k)] + potential[index(i - 1, 0, k)] +
                                                    potential[index(i, 1, k)] + potential[index(i, 0, k + 1)] +
                                                    potential[index(i, 0, k - 1)]);

            int idx1 = index(i, ny - 1, k);
            if (!fixed_mask[idx1])
                new_potential[idx1] = (1.0 / 5.0) * (potential[index(i + 1, ny - 1, k)] + potential[index(i - 1, ny - 1, k)] +
                                                     potential[index(i, ny - 2, k)] + potential[index(i, ny - 1, k + 1)] +
                                                     potential[index(i, ny - 1, k - 1)]);
        }
    }

    for (int i = 1; i < nx - 1; ++i)
    {
        for (int j = 1; j < ny - 1; ++j)
        {
            int idx = index(i, j, 0);
            if (!fixed_mask[idx])
                new_potential[idx] = (1.0 / 5.0) * (potential[index(i + 1, j, 0)] + potential[index(i - 1, j, 0)] +
                                                    potential[index(i, j + 1, 0)] + potential[index(i, j - 1, 0)] +
                                                    potential[index(i, j, 1)]);

            int idx1 = index(i, j, nz - 1);
            if (!fixed_mask[idx1])
                new_potential[idx1] = (1.0 / 5.0) * (potential[index(i + 1, j, nz - 1)] + potential[index(i - 1, j, nz - 1)] +
                                                     potential[index(i, j + 1, nz - 1)] + potential[index(i, j - 1, nz - 1)] +
                                                     potential[index(i, j, nz - 2)]);
        }
    }

    // Edge points (4 neighbors)
    for (int k = 1; k < nz - 1; ++k)
    {
        int idx = index(0, 0, k);
        if (!fixed_mask[idx])
            new_potential[idx] = (1.0 / 4.0) * (potential[index(1, 0, k)] + potential[index(0, 1, k)] +
                                                potential[index(0, 0, k + 1)] + potential[index(0, 0, k - 1)]);

        int idx1 = index(0, ny - 1, k);
        if (!fixed_mask[idx1])
            new_potential[idx1] = (1.0 / 4.0) * (potential[index(1, ny - 1, k)] + potential[index(0, ny - 2, k)] +
                                                 potential[index(0, ny - 1, k + 1)] + potential[index(0, ny - 1, k - 1)]);

        int idx2 = index(nx - 1, 0, k);
        if (!fixed_mask[idx2])
            new_potential[idx2] = (1.0 / 4.0) * (potential[index(nx - 2, 0, k)] + potential[index(nx - 1, 1, k)] +
                                                 potential[index(nx - 1, 0, k + 1)] + potential[index(nx - 1, 0, k - 1)]);

        int idx3 = index(nx - 1, ny - 1, k);
        if (!fixed_mask[idx3])
            new_potential[idx3] = (1.0 / 4.0) * (potential[index(nx - 2, ny - 1, k)] + potential[index(nx - 1, ny - 2, k)] +
                                                 potential[index(nx - 1, ny - 1, k + 1)] + potential[index(nx - 1, ny - 1, k - 1)]);
    }

    for (int j = 1; j < ny - 1; ++j)
    {
        int idx = index(0, j, 0);
        if (!fixed_mask[idx])
            new_potential[idx] = (1.0 / 4.0) * (potential[index(1, j, 0)] + potential[index(0, j + 1, 0)] +
                                                potential[index(0, j - 1, 0)] + potential[index(0, j, 1)]);

        int idx1 = index(nx - 1, j, 0);
        if (!fixed_mask[idx1])
            new_potential[idx1] = (1.0 / 4.0) * (potential[index(nx - 2, j, 0)] + potential[index(nx - 1, j + 1, 0)] +
                                                 potential[index(nx - 1, j - 1, 0)] + potential[index(nx - 1, j, 1)]);

        int idx2 = index(0, j, nz - 1);
        if (!fixed_mask[idx2])
            new_potential[idx2] = (1.0 / 4.0) * (potential[index(1, j, nz - 1)] + potential[index(0, j + 1, nz - 1)] +
                                                 potential[index(0, j - 1, nz - 1)] + potential[index(0, j, nz - 2)]);

        int idx3 = index(nx - 1, j, nz - 1);
        if (!fixed_mask[idx3])
            new_potential[idx3] = (1.0 / 4.0) * (potential[index(nx - 2, j, nz - 1)] + potential[index(nx - 1, j + 1, nz - 1)] +
                                                 potential[index(nx - 1, j - 1, nz - 1)] + potential[index(nx - 1, j, nz - 2)]);
    }

    for (int i = 1; i < nx - 1; ++i)
    {
        int idx = index(i, 0, 0);
        if (!fixed_mask[idx])
            new_potential[idx] = (1.0 / 4.0) * (potential[index(i + 1, 0, 0)] + potential[index(i - 1, 0, 0)] +
                                                potential[index(i, 1, 0)] + potential[index(i, 0, 1)]);

        int idx1 = index(i, ny - 1, 0);
        if (!fixed_mask[idx1])
            new_potential[idx1] = (1.0 / 4.0) * (potential[index(i + 1, ny - 1, 0)] + potential[index(i - 1, ny - 1, 0)] +
                                                 potential[index(i, ny - 2, 0)] + potential[index(i, ny - 1, 1)]);

        int idx2 = index(i, 0, nz - 1);
        if (!fixed_mask[idx2])
            new_potential[idx2] = (1.0 / 4.0) * (potential[index(i + 1, 0, nz - 1)] + potential[index(i - 1, 0, nz - 1)] +
                                                 potential[index(i, 1, nz - 1)] + potential[index(i, 0, nz - 2)]);

        int idx3 = index(i, ny - 1, nz - 1);
        if (!fixed_mask[idx3])
            new_potential[idx3] = (1.0 / 4.0) * (potential[index(i + 1, ny - 1, nz - 1)] + potential[index(i - 1, ny - 1, nz - 1)] +
                                                 potential[index(i, ny - 2, nz - 1)] + potential[index(i, ny - 1, nz - 2)]);
    }

    // Corner points (3 neighbors)
    if (!fixed_mask[index(0, 0, 0)])
        new_potential[index(0, 0, 0)] = (1.0 / 3.0) * (potential[index(1, 0, 0)] + potential[index(0, 1, 0)] + potential[index(0, 0, 1)]);

    if (!fixed_mask[index(nx - 1, 0, 0)])
        new_potential[index(nx - 1, 0, 0)] = (1.0 / 3.0) * (potential[index(nx - 2, 0, 0)] + potential[index(nx - 1, 1, 0)] + potential[index(nx - 1, 0, 1)]);

    if (!fixed_mask[index(0, ny - 1, 0)])
        new_potential[index(0, ny - 1, 0)] = (1.0 / 3.0) * (potential[index(1, ny - 1, 0)] + potential[index(0, ny - 2, 0)] + potential[index(0, ny - 1, 1)]);

    if (!fixed_mask[index(nx - 1, ny - 1, 0)])
        new_potential[index(nx - 1, ny - 1, 0)] = (1.0 / 3.0) * (potential[index(nx - 2, ny - 1, 0)] + potential[index(nx - 1, ny - 2, 0)] + potential[index(nx - 1, ny - 1, 1)]);

    if (!fixed_mask[index(0, 0, nz - 1)])
        new_potential[index(0, 0, nz - 1)] = (1.0 / 3.0) * (potential[index(1, 0, nz - 1)] + potential[index(0, 1, nz - 1)] + potential[index(0, 0, nz - 2)]);

    if (!fixed_mask[index(nx - 1, 0, nz - 1)])
        new_potential[index(nx - 1, 0, nz - 1)] = (1.0 / 3.0) * (potential[index(nx - 2, 0, nz - 1)] + potential[index(nx - 1, 1, nz - 1)] + potential[index(nx - 1, 0, nz - 2)]);

    if (!fixed_mask[index(0, ny - 1, nz - 1)])
        new_potential[index(0, ny - 1, nz - 1)] = (1.0 / 3.0) * (potential[index(1, ny - 1, nz - 1)] + potential[index(0, ny - 2, nz - 1)] + potential[index(0, ny - 1, nz - 2)]);

    if (!fixed_mask[index(nx - 1, ny - 1, nz - 1)])
        new_potential[index(nx - 1, ny - 1, nz - 1)] = (1.0 / 3.0) * (potential[index(nx - 2, ny - 1, nz - 1)] + potential[index(nx - 1, ny - 2, nz - 1)] + potential[index(nx - 1, ny - 1, nz - 2)]);

    // Final update
    potential = std::move(new_potential);
}

void SimulationBox3D::applyGaussSeidel()
{
    // Interior points
    for (int i = 1; i < nx - 1; ++i)
    {
        for (int j = 1; j < ny - 1; ++j)
        {
            for (int k = 1; k < nz - 1; ++k)
            {
                int idx = index(i, j, k);
                if (!fixed_mask[idx])
                {
                    potential[idx] = (1.0 / 6.0) * (potential[index(i + 1, j, k)] + potential[index(i - 1, j, k)] +
                                                    potential[index(i, j + 1, k)] + potential[index(i, j - 1, k)] +
                                                    potential[index(i, j, k + 1)] + potential[index(i, j, k - 1)]);
                }
            }
        }
    }

    // Face points (excluding edges and corners)
    for (int j = 1; j < ny - 1; ++j)
    {
        for (int k = 1; k < nz - 1; ++k)
        {
            int idx = index(0, j, k);
            if (!fixed_mask[idx])
                potential[idx] = (1.0 / 5.0) * (potential[index(1, j, k)] + potential[index(0, j + 1, k)] +
                                                potential[index(0, j - 1, k)] + potential[index(0, j, k + 1)] +
                                                potential[index(0, j, k - 1)]);

            int idx1 = index(nx - 1, j, k);
            if (!fixed_mask[idx1])
                potential[idx1] = (1.0 / 5.0) * (potential[index(nx - 2, j, k)] + potential[index(nx - 1, j + 1, k)] +
                                                 potential[index(nx - 1, j - 1, k)] + potential[index(nx - 1, j, k + 1)] +
                                                 potential[index(nx - 1, j, k - 1)]);
        }
    }

    for (int i = 1; i < nx - 1; ++i)
    {
        for (int k = 1; k < nz - 1; ++k)
        {
            int idx = index(i, 0, k);
            if (!fixed_mask[idx])
                potential[idx] = (1.0 / 5.0) * (potential[index(i + 1, 0, k)] + potential[index(i - 1, 0, k)] +
                                                potential[index(i, 1, k)] + potential[index(i, 0, k + 1)] +
                                                potential[index(i, 0, k - 1)]);

            int idx1 = index(i, ny - 1, k);
            if (!fixed_mask[idx1])
                potential[idx1] = (1.0 / 5.0) * (potential[index(i + 1, ny - 1, k)] + potential[index(i - 1, ny - 1, k)] +
                                                 potential[index(i, ny - 2, k)] + potential[index(i, ny - 1, k + 1)] +
                                                 potential[index(i, ny - 1, k - 1)]);
        }
    }

    for (int i = 1; i < nx - 1; ++i)
    {
        for (int j = 1; j < ny - 1; ++j)
        {
            int idx = index(i, j, 0);
            if (!fixed_mask[idx])
                potential[idx] = (1.0 / 5.0) * (potential[index(i + 1, j, 0)] + potential[index(i - 1, j, 0)] +
                                                potential[index(i, j + 1, 0)] + potential[index(i, j - 1, 0)] +
                                                potential[index(i, j, 1)]);

            int idx1 = index(i, j, nz - 1);
            if (!fixed_mask[idx1])
                potential[idx1] = (1.0 / 5.0) * (potential[index(i + 1, j, nz - 1)] + potential[index(i - 1, j, nz - 1)] +
                                                 potential[index(i, j + 1, nz - 1)] + potential[index(i, j - 1, nz - 1)] +
                                                 potential[index(i, j, nz - 2)]);
        }
    }

    // Edge points (4 neighbors)
    for (int k = 1; k < nz - 1; ++k)
    {
        // x edges at y=0 and y=ny-1
        int idx = index(0, 0, k);
        if (!fixed_mask[idx])
            potential[idx] = (1.0 / 4.0) * (potential[index(1, 0, k)] + potential[index(0, 1, k)] +
                                            potential[index(0, 0, k + 1)] + potential[index(0, 0, k - 1)]);

        int idx1 = index(0, ny - 1, k);
        if (!fixed_mask[idx1])
            potential[idx1] = (1.0 / 4.0) * (potential[index(1, ny - 1, k)] + potential[index(0, ny - 2, k)] +
                                             potential[index(0, ny - 1, k + 1)] + potential[index(0, ny - 1, k - 1)]);

        int idx2 = index(nx - 1, 0, k);
        if (!fixed_mask[idx2])
            potential[idx2] = (1.0 / 4.0) * (potential[index(nx - 2, 0, k)] + potential[index(nx - 1, 1, k)] +
                                             potential[index(nx - 1, 0, k + 1)] + potential[index(nx - 1, 0, k - 1)]);

        int idx3 = index(nx - 1, ny - 1, k);
        if (!fixed_mask[idx3])
            potential[idx3] = (1.0 / 4.0) * (potential[index(nx - 2, ny - 1, k)] + potential[index(nx - 1, ny - 2, k)] +
                                             potential[index(nx - 1, ny - 1, k + 1)] + potential[index(nx - 1, ny - 1, k - 1)]);
    }

    for (int j = 1; j < ny - 1; ++j)
    {
        int idx = index(0, j, 0);
        if (!fixed_mask[idx])
            potential[idx] = (1.0 / 4.0) * (potential[index(1, j, 0)] + potential[index(0, j + 1, 0)] +
                                            potential[index(0, j - 1, 0)] + potential[index(0, j, 1)]);

        int idx1 = index(nx - 1, j, 0);
        if (!fixed_mask[idx1])
            potential[idx1] = (1.0 / 4.0) * (potential[index(nx - 2, j, 0)] + potential[index(nx - 1, j + 1, 0)] +
                                             potential[index(nx - 1, j - 1, 0)] + potential[index(nx - 1, j, 1)]);

        int idx2 = index(0, j, nz - 1);
        if (!fixed_mask[idx2])
            potential[idx2] = (1.0 / 4.0) * (potential[index(1, j, nz - 1)] + potential[index(0, j + 1, nz - 1)] +
                                             potential[index(0, j - 1, nz - 1)] + potential[index(0, j, nz - 2)]);

        int idx3 = index(nx - 1, j, nz - 1);
        if (!fixed_mask[idx3])
            potential[idx3] = (1.0 / 4.0) * (potential[index(nx - 2, j, nz - 1)] + potential[index(nx - 1, j + 1, nz - 1)] +
                                             potential[index(nx - 1, j - 1, nz - 1)] + potential[index(nx - 1, j, nz - 2)]);
    }

    for (int i = 1; i < nx - 1; ++i)
    {
        int idx = index(i, 0, 0);
        if (!fixed_mask[idx])
            potential[idx] = (1.0 / 4.0) * (potential[index(i + 1, 0, 0)] + potential[index(i - 1, 0, 0)] +
                                            potential[index(i, 1, 0)] + potential[index(i, 0, 1)]);

        int idx1 = index(i, ny - 1, 0);
        if (!fixed_mask[idx1])
            potential[idx1] = (1.0 / 4.0) * (potential[index(i + 1, ny - 1, 0)] + potential[index(i - 1, ny - 1, 0)] +
                                             potential[index(i, ny - 2, 0)] + potential[index(i, ny - 1, 1)]);

        int idx2 = index(i, 0, nz - 1);
        if (!fixed_mask[idx2])
            potential[idx2] = (1.0 / 4.0) * (potential[index(i + 1, 0, nz - 1)] + potential[index(i - 1, 0, nz - 1)] +
                                             potential[index(i, 1, nz - 1)] + potential[index(i, 0, nz - 2)]);

        int idx3 = index(i, ny - 1, nz - 1);
        if (!fixed_mask[idx3])
            potential[idx3] = (1.0 / 4.0) * (potential[index(i + 1, ny - 1, nz - 1)] + potential[index(i - 1, ny - 1, nz - 1)] +
                                             potential[index(i, ny - 2, nz - 1)] + potential[index(i, ny - 1, nz - 2)]);
    }

    // Corners (3 neighbors)
    if (!fixed_mask[index(0, 0, 0)])
        potential[index(0, 0, 0)] = (1.0 / 3.0) * (potential[index(1, 0, 0)] + potential[index(0, 1, 0)] + potential[index(0, 0, 1)]);

    if (!fixed_mask[index(nx - 1, 0, 0)])
        potential[index(nx - 1, 0, 0)] = (1.0 / 3.0) * (potential[index(nx - 2, 0, 0)] + potential[index(nx - 1, 1, 0)] + potential[index(nx - 1, 0, 1)]);

    if (!fixed_mask[index(0, ny - 1, 0)])
        potential[index(0, ny - 1, 0)] = (1.0 / 3.0) * (potential[index(1, ny - 1, 0)] + potential[index(0, ny - 2, 0)] + potential[index(0, ny - 1, 1)]);

    if (!fixed_mask[index(nx - 1, ny - 1, 0)])
        potential[index(nx - 1, ny - 1, 0)] = (1.0 / 3.0) * (potential[index(nx - 2, ny - 1, 0)] + potential[index(nx - 1, ny - 2, 0)] + potential[index(nx - 1, ny - 1, 1)]);

    if (!fixed_mask[index(0, 0, nz - 1)])
        potential[index(0, 0, nz - 1)] = (1.0 / 3.0) * (potential[index(1, 0, nz - 1)] + potential[index(0, 1, nz - 1)] + potential[index(0, 0, nz - 2)]);

    if (!fixed_mask[index(nx - 1, 0, nz - 1)])
        potential[index(nx - 1, 0, nz - 1)] = (1.0 / 3.0) * (potential[index(nx - 2, 0, nz - 1)] + potential[index(nx - 1, 1, nz - 1)] + potential[index(nx - 1, 0, nz - 2)]);

    if (!fixed_mask[index(0, ny - 1, nz - 1)])
        potential[index(0, ny - 1, nz - 1)] = (1.0 / 3.0) * (potential[index(1, ny - 1, nz - 1)] + potential[index(0, ny - 2, nz - 1)] + potential[index(0, ny - 1, nz - 2)]);

    if (!fixed_mask[index(nx - 1, ny - 1, nz - 1)])
        potential[index(nx - 1, ny - 1, nz - 1)] = (1.0 / 3.0) * (potential[index(nx - 2, ny - 1, nz - 1)] + potential[index(nx - 1, ny - 2, nz - 1)] + potential[index(nx - 1, ny - 1, nz - 2)]);
}

/*
//////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////

                                DEFINE PARTICLE CLASS TO MOVE IN FIELD

//////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////
*/

struct Particle
{
    /* data */
    double x, y, z, vx, vy, vz, q, m, v, energy;
    Particle(double x = 0.0,
             double y = 0.0,
             double z = 0.0,
             double vx = 0.0,
             double vy = 0.0,
             double vz = 0.0,
             double q = 0.0,
             double m = 1.0)
    {
        this->x = x;
        this->y = y;
        this->z = z;
        this->vx = vx;
        this->vy = vy;
        this->vz = vz;
        this->q = q;
        this->m = m;
        v = sqrt(vx * vx + vy * vy + vz * vz);
        energy = 0.5 * m * v * v;
    }

    struct __init__
    {
        double x = 0.0;
        double y = 0.0;
        double z = 0.0;
        double vx = 0.0;
        double vy = 0.0;
        double vz = 0.0;
        double q = 0.0;
        double m = 1.0;

        Particle __build__() const
        {
            return Particle(x, y, z, vx, vy, vz, q, m);
        }
    };

    std::vector<double> posx;
    std::vector<double> posy;
    std::vector<double> posz;
};

void propagator(Particle &p,
                const SimulationBox3D &box,
                double t_max = 0.0,
                double dt = 0.001) // constant B field
{
    double t = 0.0;
    int steps = static_cast<int>(t_max / dt);
    double qmdt2 = (p.q / p.m) * (dt / 2.0);

    for (int step = 0; step < steps; ++step)
    {
        // Boundary check
        if (p.x < 0 || p.x >= box.lx ||
            p.y < 0 || p.y >= box.ly ||
            p.z < 0 || p.z >= box.lz)
        {
            std::cout << "Particle left the domain at step " << step << std::endl;
            break;
        }

        // Convert position to indices
        int i = static_cast<int>(p.x / box.dx);
        int j = static_cast<int>(p.y / box.dy);
        int k = static_cast<int>(p.z / box.dz);

        // Check if inside grid bounds
        if (i < 0 || i >= box.nx ||
            j < 0 || j >= box.ny ||
            k < 0 || k >= box.nz)
        {
            std::cout << "Particle reached grid edge at step " << step << std::endl;
            break;
        }

        // Check if on electrode
        if (box.fixed_mask[box.index(i, j, k)])
        {
            std::cout << "Particle hit an electrode at step " << step << std::endl;
            break;
        }

        // Interpolate E field (simple nearest-neighbor for now)
        double Ex = 0.0, Ey = 0.0, Ez = 0.0;

        // Use finite differences to approximate E = -∇φ
        if (i > 0 && i < box.nx - 1)
            Ex = -(box.potential[box.index(i + 1, j, k)] - box.potential[box.index(i - 1, j, k)]) / (2.0 * box.dx);
        if (j > 0 && j < box.ny - 1)
            Ey = -(box.potential[box.index(i, j + 1, k)] - box.potential[box.index(i, j - 1, k)]) / (2.0 * box.dy);
        if (k > 0 && k < box.nz - 1)
            Ez = -(box.potential[box.index(i, j, k + 1)] - box.potential[box.index(i, j, k - 1)]) / (2.0 * box.dz);

        // Half-step velocity
        double vx_minus = p.vx + qmdt2 * Ex;
        double vy_minus = p.vy + qmdt2 * Ey;
        double vz_minus = p.vz + qmdt2 * Ez;

        // Magnetic rotation
        double tx = qmdt2 * Bx;
        double ty = qmdt2 * By;
        double tz = qmdt2 * Bz;

        double t2 = tx * tx + ty * ty + tz * tz;
        double sx = 2 * tx / (1 + t2);
        double sy = 2 * ty / (1 + t2);
        double sz = 2 * tz / (1 + t2);

        double vpx = vx_minus + (vy_minus * tz - vz_minus * ty);
        double vpy = vy_minus + (vz_minus * tx - vx_minus * tz);
        double vpz = vz_minus + (vx_minus * ty - vy_minus * tx);

        double vx_plus = vx_minus + (vpy * sz - vpz * sy);
        double vy_plus = vy_minus + (vpz * sx - vpx * sz);
        double vz_plus = vz_minus + (vpx * sy - vpy * sx);

        // Final update
        p.vx = vx_plus + qmdt2 * Ex;
        p.vy = vy_plus + qmdt2 * Ey;
        p.vz = vz_plus + qmdt2 * Ez;

        // Position update
        p.x += p.vx * dt;
        p.y += p.vy * dt;
        p.z += p.vz * dt;

        // Save trajectory
        p.posx.push_back(p.x);
        p.posy.push_back(p.y);
        p.posz.push_back(p.z);
    }

    // Final stats
    p.v = std::sqrt(p.vx * p.vx + p.vy * p.vy + p.vz * p.vz);
    p.energy = 0.5 * p.m * p.v * p.v;
}



/*
//////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////

                                        HELPER FUNCTIONS

//////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////
*/

// get max(abs(vector<double>))
// say x=std::vector<double>{-2,3.5,6.8}
// output = 6.8
double max_AbsoluteValue_double_vector(const std::vector<double> &vec)
{
    double maxAbsValue = 0.0;

    for (double num : vec)
    {
        double absValue = std::abs(num);
        if (absValue > maxAbsValue)
        {
            maxAbsValue = absValue;
        }
    }

    return maxAbsValue;
}

// print a 1d vector
template <typename T>
void print_1d_vector(const std::vector<T> &v)
{
    for (const auto &elem : v)
    {
        std::cout << elem << " ";
    }
    std::cout << std::endl;
}

// function to concatenate double vector like np.concatenate. y=concatenate(x,y,z)
// will  give [x1,x2,x3,.... , y1,y2,y3, .... , z1,z2,z3,....]
std::vector<double> concatenate(const std::initializer_list<std::vector<double>> &vectors)
{
    std::vector<double> result;

    // Reserve total size to avoid reallocations
    size_t total_size = 0;
    for (const auto &vec : vectors)
        total_size += vec.size();
    result.reserve(total_size);

    // Insert all elements
    for (const auto &vec : vectors)
        result.insert(result.end(), vec.begin(), vec.end());

    return result;
}

// make a linspace for double value like np.linspace
std::vector<double> linspace(double start, double end, int num)
{
    std::vector<double> result(num);
    double step = (end - start) / (num - 1);
    for (int i = 0; i < num; ++i)
    {
        result[i] = start + step * i;
    }
    return result;
}

// solve a 2x2 matrix equation (helper functon)
std::pair<double, double> solve2x2(double a11, double a12, double a21, double a22, double b1, double b2)
{
    double det = a11 * a22 - a12 * a21;
    if (std::abs(det) < 1e-12)
        throw std::runtime_error("Singular matrix");

    double alpha = (b1 * a22 - b2 * a12) / det;
    double beta = (a11 * b2 - a21 * b1) / det;

    return {alpha, beta};
}

// get a exponential distribution for particle with given steepness
// more particle in low energy and less in high
std::vector<double> generate_scaled_energy(double low_energy, double high_energy, int no_of_particles, double steep)
{
    std::vector<double> raw = linspace(0.0, 10.0, no_of_particles);
    for (double &val : raw)
    {
        val = std::exp(-val / steep);
    }

    double last_raw = raw.back();

    // Linear system to get alpha, beta
    auto [alpha, beta] = solve2x2(
        low_energy, (high_energy - low_energy),
        low_energy, last_raw * (high_energy - low_energy),
        high_energy, low_energy);

    std::vector<double> Energy(no_of_particles);
    for (int i = 0; i < no_of_particles; ++i)
    {
        Energy[i] = (alpha * low_energy + beta * raw[i] * (high_energy - low_energy)) * kev_to_joule;
    }

    return Energy;
}

// save a 1d vector_double
void double_vector_save_txt(const std::vector<double> &data, const std::string &filename)
{
    std::ofstream outFile(filename);

    if (!outFile)
    {
        std::cerr << "Error: Could not open file \"" << filename << "\" for writing.\n";
        return;
    }

    for (double val : data)
    {
        outFile << val << "\n";
    }

    outFile.close();
}


// save 1d 3 double vectors of a 3d array
void save_xyz_to_txt(const std::vector<double> &x,
                     const std::vector<double> &y,
                     const std::vector<double> &z,
                     const std::string &filename)
{
    if (x.size() != y.size() || x.size() != z.size())
    {
        std::cerr << "Error: x, y, z vectors must have the same length.\n";
        return;
    }

    std::ofstream outFile(filename);
    if (!outFile)
    {
        std::cerr << "Error: Could not open file " << filename << " for writing.\n";
        return;
    }

    for (size_t i = 0; i < x.size(); ++i)
    {
        outFile << x[i] << " " << y[i] << " " << z[i] << "\n";
    }

    outFile.close();
}


// Returns a normalized vector as a tuple
std::tuple<double, double, double> normalize_vector(double vx, double vy, double vz)
{
    double norm = std::sqrt(vx * vx + vy * vy + vz * vz);

    // Avoid division by zero
    if (norm == 0.0)
        return {0.0, 0.0, 0.0};

    return {vx / norm, vy / norm, vz / norm};
}

/*
//////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////

                                        MAIN FUNCTION

//////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////
*/

int main()
{
    try
    {
        std::string configPath = "config.json";
        std::ifstream configFile(configPath);
        if (!configFile.is_open())
        {
            std::cerr << "Failed to open config file: " << configPath << std::endl;
            return 1;
        }

        json config;
        configFile >> config;

        int nx = config.value("nx", 0);
        int ny = config.value("ny", 0);
        int nz = config.value("nz", 0);
        double lx = config.value("Lx", 1.0);
        double ly = config.value("Ly", 1.0);
        double lz = config.value("Lz", 1.0);
        Bx = config.value("Bx", 1.0);
        By = config.value("By", 1.0);
        Bz = config.value("Bz", 1.0);
        std::string method = config.value("method", "jacobi");
        int max_iter = config.value("max_iter", 1000);

        SimulationBox3D box(nx, ny, nz, lx*cm, ly*cm, lz*cm);

        for (const auto &entry : fs::directory_iterator("."))
        {
            fs::path filepath = entry.path();
            std::string filename = filepath.filename().string();

            if (filepath.extension() == ".json" &&
                filename.rfind("ElectrodeConfig_", 0) == 0)
            {
                std::ifstream file(filepath);
                if (!file.is_open())
                {
                    std::cerr << "Failed to open: " << filename << "\n";
                    continue;
                }

                json j;
                try
                {
                    file >> j;
                }
                catch (const std::exception &e)
                {
                    std::cerr << "JSON parsing error in " << filename << ": " << e.what() << "\n";
                    continue;
                }

                if (!j.contains("electrodes") || !j["electrodes"].is_array() || j["electrodes"].empty())
                {
                    std::cerr << "Invalid or missing 'electrodes' in " << filename << "\n";
                    continue;
                }

                const auto &electrode = j["electrodes"][0];
                std::string type = "Unknown";
                if (electrode.contains("type") && electrode["type"].is_string())
                {
                    type = electrode["type"];
                }

                std::cout << "\nFile: " << filename << "\n";
                std::cout << "Type: " << type << "\n";

                if (type == "Plate")
                {
                    double A = electrode.value("A", 0.0);
                    double B = electrode.value("B", 0.0);
                    double C = electrode.value("C", 0.0);
                    double D = electrode.value("D", 0.0);
                    double thickness = electrode.value("thickness", 0.0);
                    double potential_val = electrode.value("potential", 0.0);

                    box.addPlane(A, B, C, D * cm, thickness * cm, potential_val);

                    // cout<<A<<B<<C<<D*cm<<thickness*cm<<potential_val<<endl;
                }

                else if (type == "Cylinder")
                {
                    double cx = electrode.value("cx", 0.0);
                    double cy = electrode.value("cy", 0.0);
                    double cz = electrode.value("cz", 0.0);
                    double radius = electrode.value("radius", 0.0);
                    double height = electrode.value("height", 0.0);
                    char axis = 'z';
                    if (electrode.contains("axis") && electrode["axis"].is_string())
                    {
                        std::string axis_str = electrode["axis"];
                        if (!axis_str.empty())
                            axis = axis_str[0];
                    }
                    double potential_val = electrode.value("potential", 0.0);

                    box.addCylinder(cx*cm, cy*cm, cz*cm, radius*cm, height*cm, axis, potential_val);
                    // cout<<cx*cm<<cy*cm<<cz*cm<<radius*cm<<height*cm<<axis<<potential_val<<endl;
                }

                else if (type == "HollowRod")
                {
                    double cx = electrode.value("cx", 0.0);
                    double cy = electrode.value("cy", 0.0);
                    double cz = electrode.value("cz", 0.0);
                    double radius = electrode.value("radius", 0.0);
                    double height = electrode.value("height", 0.0);
                    double thickness = electrode.value("thickness", 0.0);
                    char axis = 'z';
                    if (electrode.contains("axis") && electrode["axis"].is_string())
                    {
                        std::string axis_str = electrode["axis"];
                        if (!axis_str.empty())
                            axis = axis_str[0];
                    }
                    double potential_val = electrode.value("potential", 0.0);

                    box.addHollowPipe(cx*cm,cy*cm,cz*cm,radius*cm,thickness*cm,height*cm,axis,potential_val);
                    // cout<<cx*cm<<cy*cm<<cz*cm<<radius*cm<<height*cm<<axis<<potential_val<<endl;
                }

                else if (type == "Box")
                {
                    double x0 = electrode.value("x0", 0.0);
                    double y0 = electrode.value("y0", 0.0);
                    double z0 = electrode.value("z0", 0.0);
                    double x1 = electrode.value("x1", 0.0);
                    double y1 = electrode.value("y1", 0.0);
                    double z1 = electrode.value("z1", 0.0);
                    double potential_val = electrode.value("potential", 0.0);

                    box.addBox(x0*cm,y0*cm,z0*cm,x1*cm,y1*cm,z1*cm,potential_val);
                    // cout<<cx*cm<<cy*cm<<cz*cm<<radius*cm<<height*cm<<axis<<potential_val<<endl;
                }

                else if (type == "Spherical")
                {
                    double cx = electrode.value("cx", 0.0);
                    double cy = electrode.value("cy", 0.0);
                    double cz = electrode.value("cz", 0.0);
                    double radius = electrode.value("radius", 0.0);
                    double potential_val = electrode.value("potential", 0.0);

                    box.addSphere(cx*cm,cy*cm,cz*cm,radius*cm,potential_val);
                    // cout<<cx*cm<<cy*cm<<cz*cm<<radius*cm<<height*cm<<axis<<potential_val<<endl;
                }

                else if (type == "Ellipsoidal")
                {
                    double cx = electrode.value("cx", 0.0);
                    double cy = electrode.value("cy", 0.0);
                    double cz = electrode.value("cz", 0.0);
                    double rx = electrode.value("rx", 0.0);
                    double ry = electrode.value("ry", 0.0);
                    double rz = electrode.value("rz", 0.0);
                    double potential_val = electrode.value("potential", 0.0);

                    box.addEllipsoid(cx*cm,cy*cm,cz*cm,rx*cm,ry*cm,rz*cm,potential_val);
                    // cout<<cx*cm<<cy*cm<<cz*cm<<radius*cm<<height*cm<<axis<<potential_val<<endl;
                }

                else if (type == "Hyperboloidal")
                {
                    double cx = electrode.value("cx", 0.0);
                    double cy = electrode.value("cy", 0.0);
                    double cz = electrode.value("cz", 0.0);
                    double a = electrode.value("a", 0.0);
                    double b = electrode.value("b", 0.0);
                    double c = electrode.value("c", 0.0);
                    double waist = electrode.value("waist", 0.0);
                    char axis = 'z';
                    if (electrode.contains("axis") && electrode["axis"].is_string())
                    {
                        std::string axis_str = electrode["axis"];
                        if (!axis_str.empty())
                            axis = axis_str[0];
                    }
                    double potential_val = electrode.value("potential", 0.0);

                    box.addHyperboloid(cx*cm,cy*cm,cz*cm,a*cm,b*cm,c*cm,waist*cm,axis,potential_val);
                    // cout<<cx*cm<<cy*cm<<cz*cm<<radius*cm<<height*cm<<axis<<potential_val<<endl;
                }

                else
                {
                    std::cout << "  [!] Unknown electrode type. Skipping parsing.\n";
                }
            }
        }

        // Save geometry
        double_vector_save_txt(box.geometry, "geometry.txt");

        return 0;
    }
    catch (const std::exception &e)
    {
        std::cerr << "Error: " << e.what() << std::endl;
        return 1;
    }
}