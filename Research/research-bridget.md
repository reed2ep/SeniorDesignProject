# Path Finding Algorithm Notes

**A\***  
- Uses heuristics to estimate the distance between the current node and the destination node. Scores for nodes are calculated based on distance from the starting point and an estimate of remaining distance (heuristic function). The smallest score is chosen, and the process is repeated with neighboring nodes until it reaches the destination.  
- If a node is revisited with a smaller distance, the distance and path are updated.

**Pros**:  
- Guaranteed shortest path  
- Heuristics make it more efficient  
- Guaranteed to expand the fewest nodes  

**Cons**:  
- Depends on the quality and accuracy of the heuristic function



**Dijkstra’s**  
- Starting node is assigned a distance of 0 and distance is calculated to neighboring nodes. The node with the smallest distance is selected. Repeated with the newly added node until it reaches the destination.  
- If a node is revisited with a smaller distance, the distance and path are updated.

**Pros**:  
- Guaranteed shortest path  
- Accounts for all potential paths  
- Does not rely on a heuristic  

**Cons**:  
- Computational cost increases with graph size  
- Not as efficient on larger or more complex graphs  



**K-shortest**  
- K – number of shortest paths to compute  
- For k=1, the algorithm uses another algorithm to find the shortest path, commonly Dijkstra’s. The used algorithm is then extended so for k=n, the algorithm returns at most n paths, the best path and n-1 deviations of the best path (in increasing order of cost).  
- An already discovered shortest path will not be traversed again.  

**Pros**:  
- Provides multiple path options  

**Cons**:  
- Computation cost is increased since it is finding multiple paths  
- Not as efficient on larger or more complex graphs



# Database Management Notes
