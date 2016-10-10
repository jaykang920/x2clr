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


## Event communication 

### xpiler 

### subscribe / publishing 

## Case development 








  


 

     