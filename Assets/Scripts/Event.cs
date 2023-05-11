using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//============ OO Pattern: Observer ============
public class Event {
    public enum eventType { PowerPellet, AISwap, AIModeChange, Panic, Pause };
    bool pelletActive;
    eventType currentType;
    AIMode newMode;

    public Event(eventType type, bool pelletActive = false, AIMode mode = AIMode.Scatter){
        currentType = type;
        this.pelletActive = pelletActive;
        newMode = mode;
    }

    public eventType getEventType(){
        return currentType;
    }
    public bool pelletIsActive(){
        return pelletActive;
    }
    public AIMode getNewMode(){
        return newMode;
    }
}

public interface Observer {
    void eventUpdate(Event newEvent);
}

public interface Subject {
    void notifySubscribers(Event newEvent);
    void subscribe(Observer subscriber);
    void unsubscribe(Observer subscriber);
}