using UnityEngine;
using System.Collections;

interface Possessable {

    void bodyTransition(bool entering);
    void startControlling();
    void stopControlling();
}
