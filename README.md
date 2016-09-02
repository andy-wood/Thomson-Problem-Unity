# Thomson-Problem-Unity
An interactive Monte Carlo hill climbing solver and visualization for Thomson's Problem

Controls: 

R - rotate the figure

, / < - subtract a point

. / > - add a point

The Thompson Problem is about find the stable, minimum-energy configuration of electrons as they repel each other on a sphere. However, before I knew about Thompson's Problem, I conceived of it as simply finding the configuration of N points in 3D space that is most analogous to an equilateral triangle. For 4 points, this is a regular tetrahedron. For more than 4 points, there is no solution in 3D with every point pair equidistant (such a solution is a k-simplex in successively higher dimensions). But there is a solution that minimizes the sum of the differences between the lengths of every point pair. The surface of this figure may or may not be equilateral. This turns out to be very similar to Thompson's Problem, except that certain solutions so far seem to be elipsoidal (I believe most are spherical or nearly spherical, though).

The program continuously searches for a better solution, while the rendered spheres (points) continuously move towards the latest best solution.
