#!/usr/bin/env python3

# Dimensions of the image
W = 25
H = 6
SIZE = W * H

with open("input") as f:
    data = f.readline().strip()

print("Day 08")

# Part 1
test = [(data.count('0', i, i + SIZE), data.count('1', i, i + SIZE) * data.count('2', i, i + SIZE)) for i in range(SIZE, len(data), SIZE)]
test.sort()
print(f"1. {test[0][1]}")

# Part 2
# Define characters for output
black = u"\u2588"
white = " "
trans = "@"

# Build the image layer by layer
image = [trans] * SIZE
for i in range(len(data)):   
    if image[i % SIZE] == trans and data[i] != '2':
        image[i % SIZE] = black if data[i] == '1' else white

# Print ascii art
print("2. Image:")
for y in range(H):
    print(''.join(image[y * W:y * W + W]))