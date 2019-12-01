#!/usr/bin/env python3

def sum_fuel (mass):
    fuel = mass // 3 - 2
    total = 0
    while fuel > 0:
        total += fuel
        fuel = fuel // 3 - 2
    return total

# Read data
with open("input") as f:
    data = f.readlines()

# Part 1
fuel = sum([int(n) // 3 - 2 for n in data])
print(f"1. Total Fuel: {fuel}")

# Part 2
fuel = sum([sum_fuel(int(n)) for n in data])
print(f"2. Total Fuel: {fuel}")