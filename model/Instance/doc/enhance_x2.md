# Session Management 

## Lock 

 - TcpServer keeps a list of sessions with a rwlock 
 - Dispatch uses rwlock to build a handler chain every time.  

## Reconnection 
 
 - Better to have reentrance into the service / instance instead of session recovery in x2 layer
 - Implement a sample  

# Scheduling 

 - More efficient timer management
 - Task scheduling

# Optimization 

 -   

# Game layer 

 - Add Unity emulation layer 
 - Use it as a base for game dev.
 - Add Entity / Component system
 


 