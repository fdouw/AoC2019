#!/usr/bin/env python3

class Node:
    sanTrace = False
    def __init__(self,name,parent=None):
        self.name = name
        self.parent = parent
    def orbit_count(self):
        return 0 if not self.parent else self.parent.orbit_count() + 1

with open("input") as f:
    data = [s.strip().split(')') for s in f.readlines()]

# Create graph of the orbits
orbits = dict()
for parent,child in data:
    if not parent in orbits:
        orbits[parent] = Node(parent)
    if not child in orbits:
        orbits[child] = Node(child)
    orbits[child].parent = orbits[parent]

print("Day 06")

# Part 1: count all orbits
print(f"1. {sum(n.orbit_count() for n in orbits.values())}")

# Part 2: find common root orbit (CRO).  Transfers needed is the number of orbits between You and
# the CRO and between Santa and the CRO.
sanTrace = orbits['SAN']
youTrace = orbits['YOU']
while sanTrace.parent:
    sanTrace = sanTrace.parent
    sanTrace.sanTrace = True
while youTrace.parent:
    youTrace = youTrace.parent
    if youTrace.sanTrace:
        transfers = orbits['SAN'].orbit_count() + orbits['YOU'].orbit_count() - 2 * youTrace.orbit_count() - 2
        break
print(f"2. {transfers}")

