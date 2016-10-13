# Instance Based Games 

## Structure 

 - Master server to manage users and instances
 - Game server to run instances 
 - Session server to process communication with clients 
 - Couchbase for persistence

# Master 

 - Active / Standby replication by replay packets 
 - Directories
    - Server 
    - user
    - Instance

 - Scalability with addition of memory  

## Clustering 

 - An active node process and responds
 - The active node relays packets to the stanby node. 
 - A standy node connects to the active node and reports its existence. 
 - A standby node process and ignore repsonse. 
 - When an active node crashes, then standby node becomes active. 
  
## Directory 

 - Indexing on various fields 
 - Fast search with index. 


# Session 

## Client checkin process

 - A list of well known session servers are included in client. 
 - A client selects a session server randomly from the list. 
 - A client connects to the selected session server and gets list of all session servers. Then, update the list for later use.  
    - If a client fails to connect to the server, then tries the next one till it fails with all session servers.
 - Client authenticates with the connected session server.  

## Instance creation / search / join 

 - Session receives requests from client 
 - Then work with a master to handle instance management.  
 - Directory<Instance> is an actor (flow in x2 term) and there are several Directory for instance handling. 
 - Directory<Instance> works with a Game server to create / join / leave / destroy instances

# Game 

 - Runs instances. 
 - Handles game packets

# Couchbase 

 - A actor that can handle couchbase query 
 - Key based sequence guarantee
 - Distributes load to tasks 

# Log 
 
 - NLog w/ async io 
 - Kinesis 
 
# Building 

 - Unit tests / Functional tests in NUnit
    - More examples into x2 to learn and practice
 - Simulated clients in WPF and C#
    - Performance reports (response time / load on server)
 - Manager / monitor in WPF

## Plan 

  -  
