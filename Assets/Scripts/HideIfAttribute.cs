using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

// [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class HideIfAttribute : PropertyAttribute {
    public enum DisableType
    {
        ReadOnly = 2,
        DontDraw = 3
    }
    public enum ComparisonType 
    {
        Greater,
        Less,
        Equal,
        NotEqual,
        GreaterOrEqual,
        LessOrEqual
    }
    public object arg1 { get; private set; }
    public ComparisonType comparisonType;
    public object arg2 { get; private set; }
    // public Func<bool> comparison { get; private set; }
    public DisableType disableType { get; private set; }

    public HideIfAttribute(object arg1, object arg2, DisableType disableType = DisableType.DontDraw) {
        this.arg1 = arg1;
        this.arg2 = arg2;
        this.comparisonType = ComparisonType.Equal;
        this.disableType = disableType;
    }
    public HideIfAttribute(object arg1, ComparisonType comparisonType, object arg2, DisableType disableType = DisableType.DontDraw) {
        this.arg1 = arg1;
        this.arg2 = arg2;
        this.comparisonType = comparisonType;
        this.disableType = disableType;
    }
    public HideIfAttribute(object arg1, string comparison, object arg2, DisableType disableType = DisableType.DontDraw) {
        this.arg1 = arg1;
        this.arg2 = arg2;

        comparison = new string(comparison.Where(c => c != ' ').ToArray());

        ComparisonType comparisonType = ParseComparisonType(comparison);

        this.comparisonType = comparisonType;


        this.disableType = disableType;
    }
    public HideIfAttribute(string condition, DisableType disableType = DisableType.DontDraw) {
        this.disableType = disableType;

        string[] arguments = condition.Split(' ').ToArray(); //Split string by spaces
        arguments = arguments.Where(a => a != "").ToArray(); //Remove blank arguments

        if(arguments.Length == 2) {
            this.arg1 = arguments[0];
            this.arg2 = arguments[1];
            this.comparisonType = ComparisonType.Equal;
        }
        else if(arguments.Length == 3) {
            this.arg1 = arguments[0];
            this.comparisonType = ParseComparisonType(arguments[1]);
            this.arg2 = arguments[2];
        }
        else {
            MonoBehaviour.print("String condition was not valid");
            this.arg1 = "arg1";
            this.arg1 = "arg2";
            this.comparisonType = ComparisonType.Equal;
        }
    }

    ComparisonType ParseComparisonType(string str) {
        if(str == "=" || str == "==") return ComparisonType.Equal;
        else if(str == "!=") return ComparisonType.Equal;
        else if(str == "<") return ComparisonType.Less;
        else if(str == ">") return ComparisonType.Greater;
        else if(str == "<=") return ComparisonType.LessOrEqual;
        else if(str == ">=") return ComparisonType.GreaterOrEqual;
        else return ComparisonType.Equal;
    }
}