# API for finding (almost) optimal anchorage spots for fleets
In this project, I'm working on finding the optimal way to arrange ships on an anchorage, such that we can place every ship using as few iterations of the anchorage as possible.

# How to use API

By sending a JSON object, in the format given by Instech, to localhost:{PORT}/GetOptimalAnchorages, you will receive a plaintext response telling you the amount of iterations needed, and a visual representation of the ship placement according to the algorithm used.

Use http://localhost:PORT/swagger/index.html to access API.

# Algorithm

Tries to place ships as far up as possible, starting at the left side, moving right. Tries to place longest ships first, then shorter.

# Weaknesses in algorithm

Not guaranteed to give an optimal solution, but should still be pretty good. 

# Weaknesses in code

General functionality and security-issues, such as escaping inputs.
Some leftover generic code.
