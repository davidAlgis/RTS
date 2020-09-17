using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[System.Serializable]
public class Resources
{
    public uint wood;
    public uint gold;
    public uint rock;
    public uint food;

    #region constructor
    public Resources(uint w, uint g, uint r, uint f)
    {
        wood = w;
        gold = g;
        rock = r;
        food = f;
    }

    public Resources(uint x)
    {
        wood = x;
        gold = x;
        food = x;
        rock = x;
    }

    public Resources()
    {
        wood = 0;
        gold = 0;
        rock = 0;
        food = 0;
    }
    #endregion

    /*Give what is really substract. For example considering only one dimension :
     We use uint therefore if we have r1.wood = 8, r2.wood = 10. If we do r1-r2 
     we'll obtain 0 and not -2. Therefore it's interesting to what we really substract
     here it was 8. */
    public static Resources realSubstract(Resources r1, Resources r2)
    {
        Resources result = new Resources();

        if (r1.wood > 0)
            if (r1.wood - r2.wood > 0)
                result.wood = r2.wood;
            else
                result.wood = r1.wood;
       
        if (r1.food > 0)
            if (r1.food - r2.food > 0)
                result.food = r2.food;
            else
                result.food = r1.food;

        if (r1.gold > 0)
            if (r1.gold - r2.gold > 0)
                result.gold = r2.gold;
            else
                result.gold = r1.gold;

        if (r1.rock > 0)
            if (r1.rock - r2.rock > 0)
                result.rock = r2.rock;
            else
                result.rock = r1.rock;
    

        return result; 
    }

    
    #region overloadOperator
    public static Resources operator -(Resources r1, uint x)
    {
        Resources result = new Resources();
        if (r1 == null)
        {
            Debug.LogWarning("Unable to add resources and uint, because the resources isn't set.");
            return new Resources();
        }

        if (r1.wood >= x)
            result.wood = r1.wood - x;
        else
            result.wood = 0;

        if (r1.food >= x)
            result.food = r1.food - x;
        else
            result.food = 0;

        if (r1.gold >= x)
            result.gold = r1.gold - x;
        else
            result.gold = 0;

        if (r1.rock >= x)
            result.rock = r1.rock - x;
        else
            result.rock = 0;

        return result;
    }

    public static Resources operator +(Resources r1, uint x)
    {
        Resources result = new Resources();
        if (r1 == null)
        {
            Debug.LogWarning("Unable to add resources and uint, because the resources isn't set.");
            return result;
        }
        result.wood = r1.wood + x;
        result.food = r1.food + x;
        result.gold = r1.gold + x;
        result.rock = r1.rock + x;

        return result;
    }

    public static Resources operator /(Resources r1, float x)
    {
        Resources result = new Resources();
        if (r1 == null)
        {
            Debug.LogWarning("Unable to add resources and uint, because the resources isn't set.");
            return result;
        }

        if(x==0)
        {
            Debug.LogWarning("Try to divide by 0");
            return result;
        }

        result.wood = (uint)((float)r1.wood / x);
        result.food = (uint)((float)r1.food / x);
        result.gold = (uint)((float)r1.gold / x);
        result.rock = (uint)((float)r1.rock / x);

        return r1;
    }

    public static Resources operator *(Resources r1, float x)
    {
        Resources result = new Resources();
        if (r1 == null)
        {
            Debug.LogWarning("Unable to add resources and uint, because the resources isn't set.");
            return result;
        }

        result.wood = (uint)((float)r1.wood * x);
        result.food = (uint)((float)r1.food * x);
        result.gold = (uint)((float)r1.gold * x);
        result.rock = (uint)((float)r1.rock * x);
        return result;
    }

    public static Resources operator +(Resources r1, Resources r2)
    {
        Resources result = new Resources();
        if (r1 == null || r2 == null)
        {
            Debug.LogWarning("Unable to add two resources, because one/two of them isn't set.");
            return result;
        }

        result.wood = r1.wood + r2.wood;
        result.food = r1.food + r2.food;
        result.gold = r1.gold + r2.gold;
        result.rock = r1.rock + r2.rock;

        return result;
    }

    public static Resources operator -(Resources r1, Resources r2)
    {
        Resources result = new Resources();
        if (r1 == null || r2 == null)
        {
            Debug.LogWarning("Unable to add two resources, because one/two of them isn't set.");
            return result;
        }

        if (r1.wood >= r2.wood)
            result.wood = r1.wood - r2.wood;
        else
            result.wood = 0;

        if (r1.food >= r2.food)
            result.food = r1.food - r2.food;
        else
            result.food = 0;

        if (r1.gold >= r2.gold)
            result.gold = r1.gold - r2.gold;
        else
            result.gold = 0;

        if (r1.rock >= r2.rock)
            result.rock = r1.rock - r2.rock;
        else
            result.rock = 0;

        return result;
    }

    public static bool operator==(Resources r1, Resources r2)
    {
        if ((object)r1 == null)
            return (object)r2 == null;

        return r1.Equals(r2);
    }

    public static bool operator !=(Resources r1, Resources r2)
    {
        return !(r1 == r2);
    }

    public static bool operator >(Resources r1, Resources r2)
    {
        if ((object)r1 == null)
            return (object)r2 == null;

        return r1.wood > r2.wood && r1.food > r2.food && r1.gold > r2.gold && r1.rock > r2.rock;
    }

    public static bool operator <(Resources r1, Resources r2)
    {
        if ((object)r1 == null)
            return (object)r2 == null;

        return r1.wood < r2.wood && r1.food < r2.food && r1.gold < r2.gold && r1.rock < r2.rock;
    }

    public static bool operator >=(Resources r1, Resources r2)
    {
        if ((object)r1 == null)
            return (object)r2 == null;

        return r1.wood >= r2.wood && r1.food >= r2.food && r1.gold >= r2.gold && r1.rock >= r2.rock;
    }

    public static bool operator <=(Resources r1, Resources r2)
    {
        return !(r1 >= r2);
    }

    public static bool operator >(Resources r1, uint x)
    {
        return r1 > new Resources(x);
    }

    public static bool operator <(Resources r1, uint x)
    {
        return r1 < new Resources(x);
    }

    public static bool operator >=(Resources r1, uint x)
    {
        return r1 >= new Resources(x);
    }

    public static bool operator <=(Resources r1, uint x)
    {
        return r1 <= new Resources(x);
    }


    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        Resources r1 = (Resources)obj;

        return wood == r1.wood && food == r1.food && gold == r1.gold && rock == r1.rock;
    }

    public override string ToString()
    {
        if (this == null)
            return "The ressources is null";
        
        if(wood != 0)
            if (gold == 0 && food == 0 && rock == 0)
                return wood.ToString();

        if (gold != 0)
            if (wood == 0 && food == 0 && rock == 0)
                return gold.ToString();
        

        if (food != 0)
            if (wood == 0 && gold == 0 && rock == 0)
                return food.ToString();

        if (rock != 0)
            if (wood == 0 && gold == 0 && food == 0)
                return rock.ToString();

        return "Wood : " + wood + " Food : " + food + " Gold : " + gold + " Rock " + rock;
    }

    public override int GetHashCode()
    {
        return ShiftAndWrap(wood.GetHashCode(), 2) ^ food.GetHashCode() ^ ShiftAndWrap(gold.GetHashCode(), 2) ^ rock.GetHashCode();
    }

    //This function is used to do a good getHashCode
    private int ShiftAndWrap(int value, int positions)
    {
        positions = positions & 0x1F;

        // Save the existing bit pattern, but interpret it as an unsigned integer.
        uint number = System.BitConverter.ToUInt32(System.BitConverter.GetBytes(value), 0);
        // Preserve the bits to be discarded.
        uint wrapped = number >> (32 - positions);
        // Shift and wrap the discarded bits.
        return System.BitConverter.ToInt32(System.BitConverter.GetBytes((number << positions) | wrapped), 0);
    }
    #endregion
}
