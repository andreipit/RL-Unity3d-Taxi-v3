using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GymLibrary
{


    public enum Rows { one, two, three, four, five }
    public enum Cols { one, two, three, four, five }
    public enum PassengerLocations { R, G, B, Y, Taxi }
    public enum DestinationLocations { R, G, B, Y }

    public enum TaxiPosition { R, G, B, Y, Other }


    [Serializable]
    public class BoxObservation
    {

        public Rows TaxiRow;
        public Cols TaxiCol;
        public PassengerLocations PassLoc;
        public DestinationLocations DestLoc;
        public TaxiPosition DebugTaxiLoc;

        public BoxObservation(Rows _Row, Cols _Col, PassengerLocations _PassLoc, DestinationLocations _DestLoc)
        {
            TaxiRow = _Row;
            TaxiCol = _Col;
            PassLoc = _PassLoc;
            DestLoc = _DestLoc;
            DebugTaxiLoc = GetTaxiPos();
        }

        public string Print => "Row=" + TaxiRow + " Col=" + TaxiCol + " PassLoc=" + PassLoc + " DestLoc=" + DestLoc;

        public TaxiPosition GetTaxiPos()
        {
            if (TaxiRow == Rows.one && TaxiCol == Cols.one) return TaxiPosition.R;
            if (TaxiRow == Rows.one && TaxiCol == Cols.five) return TaxiPosition.G;
            if (TaxiRow == Rows.five && TaxiCol == Cols.four) return TaxiPosition.B;
            if (TaxiRow == Rows.five && TaxiCol == Cols.one) return TaxiPosition.Y;
            return TaxiPosition.Other;

        }

        public bool IsPickupLegal()
        {
            if (PassLoc == PassengerLocations.Taxi)
            {
                //Debug.Log("Error: passen in taxe: 1=" + PassLoc.ToString() + " 2=" + PassengerLocations.Taxi.ToString());
                return false;
            }

            if (GetTaxiPos().ToString() != PassLoc.ToString())
            {
                //Debug.Log("Error: taxi is not near: 1=" + GetTaxiPos().ToString() + " 2=" + PassLoc.ToString());
                return false;

            }

            return true;
        }

    }
}