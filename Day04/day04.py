#!/usr/bin/env python3

def match(num):
    s = str(num)
    for i, c in enumerate(s):
        if i > 0 and s[i-1] > c:
            return -1
    l = list(s)
    counts = set(l.count(c) for c in set(s))
    return 2 if 2 in counts else max(counts)

digit_counts = list(match(n) for n in range(137683,596253))

print("Day 04")
print(f"1. {sum(1 for n in digit_counts if n > 1)}")
print(f"2. {sum(1 for n in digit_counts if n == 2)}")