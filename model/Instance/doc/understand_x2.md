# Unit Tests 

 - Binder, Buffer, Capo, Cell, Cipher, Config, Fingerprint, Handler, Hash, Pool, Sample, Serializer
 - They are not sufficient to fully cover 

## Guide 

### Refactoring Best / Worst

- [Material] (http://blog.stevensanderson.com/2009/08/24/writing-great-unit-tests-best-and-worst-practises/) 

#### Gist

 - Designing software components robustly
 - TDD is a design process, not a testing process
 - Changes to other parts of your codebase don’t make them(unit tests) start failing 


# HelloWorld in Examples

 - xpiler generates Event subclass from xml definition 
 - Event is a subclass of Cell 
 - Cell has a Tag and a Fingerprint 
    - Cell has several equal functions: 
        - Equals, EqualsTo
        - Equivalent(Cell), Equivalent(Cell, Finterprint)
    - Tag has Base tag, NumProps, 

## Fingerprint Unit Test 

 Fingerprint is a bit vector of fixed length. 

 block (0~31 bit) | blocks (higher than 32bit)

### Equivalent 

 Def. A is equivalent to B, if Fingerprint A covers and equals all bits of another Fingerprint B. 

### Deserialize

 It seems that desrialize has a bug or incomplete since: 
  - it does not set length and it does not allocate blocks.  
  
## Buffer Unit Test

 - Segment / SegmentedBuffer / SegmentPool are related. 
 - Buffer uses SegmentPool. 
 - Correctness proof is required.    
 - Internal workings are tighltly dependent on call order. 
    - MarkToRead, Trim, and so on. 

 - Consider Buffer as an internal class. 

## Capo Unit Test 

 - What is Capo? It seems to be borrowed from guitar Capo. 
 - Capo dispaces index by provided offset.  
 - It is used mostly with Fingerprint to access indexed value with modified offset.

## Cell Unit Test

 - Cell has a Tag and a Fingerprint
 - Base class does not do much and it is incomplete. 
 - It will become complete with Event 

### SampleCell1

 - Tag : 2 properties. No base. typeof(SampleCell1) 
 - Tag and Fingerprint enables to track field modification from subclassing tree. 
 - Fingerprint is used to serialize only fields that are touched. 
   - Is this useful? It can cause sensetive bugs in communication. (Code level protocol) 
 
### TestEquality() 

 - Equals() checks EqualsTo() on both objects. 

### TestHashing() 

 - GetHashCode() reflected Fingerprint Touch and Field values. 
 - Following two have different hash code since Bar field's value is different. 
    - var cell2 = new SampleCell1 { Foo = 1, Bar = "bar" };
    - var cell3 = new SampleCell1 { Foo = 1, Bar = "foo" };

### TestEquivalence() 

 - Two cells are equivalent if : 
    - their fingerprints are equivalent 
    - their field values are same when checked by fingerprint

 - Summarize the concept??  

## Binder Unit Test 

 - Event based dispatching 
 - TypeId + Fingerprint based dispatching 
 - Base event handler dispatching using typeid from Tag.   

# Hub / Flow / Case 

## Startup / Teardown 

 - Very simple. Just provide with Hub.Attach(), Flow.Add() 

## EventBasedFlow / SingleThreadedFlow 

 - Waits in queue.Dequeue() till an event arrives

## Event dispatching 

 - Hub: Channel (string) based filtering of flows or broadcast to all flows 
 - Then, typeid / fingerprint based dispatching.
 - string channel is good enough? 

 - Instance based (Value based) dispatching is absolutely required. 

### Value based dispatching 

 - It works and following is why: 
   - Binder::BuildHandlerChain() has handlerMap.TryGetValue(equivalent, out handlers) 
   - Then TryGetValue uses HashCode of equivalent which reflects hash value of Fingerprint. 
   - Then Fingerprint reflects value of assigend field value. 
   - This is an ingenious structure. But it is rather difficult to understand at first and debug. 

# xpiler 

 - very simple. 
 - more examples required   
    - inheritance 
    - class / struct type field


# Distribution through Tcp

## Hello Example to TestFuncTcpSimple 

### AsyncTcpServer 

 - AbstractTcpServer 
    - Accept and manages LinkSession
 - ServerLink 
    - Has LinkSessions
    - Broadcast 
    - Send 
 - SessionBasedLink 
    - Has Preprocess. This can be used to modify Events. 
 - Link 
 - Case 

#### TestFuncTcpSimple

 - Use timer to periodically send event 
 - Use channel to differentiate client event from server response 


#### Q1. How to bind / post events from other Cases using TcpServer? 

 - Receiving is simple with Flow event subscription. 
 - Sending is through Flow to TcpServer Case, then to the LinkSession. 
 - Sending flow is not natural and there is queueing delay. 
 - Reported issue 12. 

A1. Multiple Flows, Hub channels, and Preprocess delegate can solve the issue. 

 - Add an example with a functional test. 

## Server session management 

 - Connecting other servers
 - Detecting other servers
 - Detecting disconnection 
 
### TestFuncSessionManagement 

 - SampleClientCase connects and communicates NodeJoin / NodeLeave with Name 
  
  
  

### TimeFlow 

 - FrameBasedFlow 
    - Has a Thread
    - Calls Run 
    - Run calls Update

 - Periodic processing functionality 
    - With a timer
    - Call Reserve
    - TimeFlow::Update calls Timer::Tick() 
    - No queue processing intended

### Usage Example 

  - Use TimeFlow.Default timeflow 
  - Register Periodic event with Case Instance Id 
  - Use it to periodically run the Case Instance.
  - To cancel timer, original event needs to be kept. 

The above scheme can be used to schedule most games. 
Games with lots of entities need different scheduling scheme.


# Game Dev. - Instance Model 

## Session Server 

 - SingleThreadFlow()
 - TcpServer()
 - SessionCase 
    - Reactive 
        - Authorize 
        - Join / leave / create instance 
    - Active 

## Game Server 

 - SingleThreadFlow() 
    - Assign channel 



  


 

     