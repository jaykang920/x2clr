# Goal 

 - Billion concurrent users for instance based games. 
 
# Solution 

## Authentication / Authorization (aka. Login)

 - AuthCases running on serveral machines
 - Requests are passed to the AuthCase with hashing of account key  

## Matching 

 - MatchCases running on serveral machines 
 - MatchCases have its own Zone by design
 - Each MatchCase knows InstanceRunners by report 
 - Each MatchCase can process 100,000 requests per second 
   - Try and test with 1,000,000 requests / sec.
 - Boot time is 2.7 hours with a single MatchCase
 
## Wishes 

 - Dynamic addition and removal of Cases 
 - No maintenance downtime

## Ideation

 - Instances can be easily distributed over multiple servers. 
 - Bottleneck is usually on servers that require global knowledge
   - Authentication / authorization, matching, and ranking are the well known ones.
   
### Authentication / Authorization

 - Billion entries with 1K information reaches 100G bytes

 Idea 1: 
 - Cached directory with SSD DBM style db.
 - A single machine cannot process during boot time. 
 - If login processing rate is 100,000 per sec., then total time is 2.77 hours. 
 - Therefore, this cannot be a solution. 
 
 Idea 2: 
 - Partition auth service with account string hashing. 
 - several servers can be used and decrease the booting time.   
 - 16GB memory, 10 machines can make the booting time 15 minutes.
 - This can be the solution
 
#### Solution  

 100 AuthCase running on serveral machines partitioned with account key.
 
 Wish: 
 - Reconfiguration of AuthCase 
   - Add / Remove 
   - The most basic problem of distribution
 
### Matching 

 Processing delay and arrival rate of requests are very important. 

 - 100,000 per sec can be implemented
   
#### Solution 

 Serveral MatchCase partitioned with game design can support one billion.
 
### Instance 

 Dynamic addition of instance runners
 Least N instance runner random selection
 
 
 
  