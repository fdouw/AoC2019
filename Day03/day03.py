#!/usr/bin/env python3
# Shamelessly copied from /u/jonathan_paulson (and a bit from /u/mcpower_).  My own version would be
# along the lines of my c# implementation, which is clearly less elegant.

dx = dict(zip('RULD',[1,0,-1,0]))
dy = dict(zip('RULD',[0,1,0,-1]))

def get_points(directions):
    x, y = 0,0  # Current position in the grid
    pts = {}    # Contains the min. distance travelled for each point visited
    length = 0  # The length of the path thusfar
    for cmd in directions:
        side = cmd[0]
        dist = int(cmd[1:])
        for _ in range(dist):
            x += dx[side]
            y += dy[side]
            length += 1
            if not (x,y) in pts.keys():
                pts[(x,y)] = length
    return pts

with open("input") as f:
    A,B = f.readlines()

# Compute the points each path travels, and the points where the paths intersect
path_a = get_points(A.split(','))
path_b = get_points(B.split(','))
intersects = path_a.keys() & path_b.keys()

print("Day 03")
print(f"1. {min(abs(x) + abs(y) for x,y in intersects)}")
print(f"2. {min(path_a[p] + path_b[p] for p in intersects)}")