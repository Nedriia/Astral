using UnityEngine;
using System.Collections;

interface Possessable {

    float bodyTransition(bool entering);
    void startControlling();
    void stopControlling();
}
