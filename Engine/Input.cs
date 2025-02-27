//            Copyright Keysight Technologies 2012-2019
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, you can obtain one at http://mozilla.org/MPL/2.0/.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace OpenTap
{
    /// <summary>   Interface for TestStep input properties. </summary>
    public interface IInput
    {
        /// <summary>   Gets or sets the TestStep that has the output property to which this Input is connected. </summary>
        ITestStep Step { get; set; }

        /// <summary>   Describes the <see cref="OutputAttribute"/> property on the <see cref="Step"/> to which this Input is connected.   </summary>
        IMemberData Property { get; set; }
    }

    /// <summary>
    /// Input type restriction for IInput.
    /// </summary>
    public interface IInputTypeRestriction : IInput
    {
        /// <summary> returns true if the concrete type is supported. </summary>
        /// <param name="concreteType"></param>
        /// <returns></returns>
        bool SupportsType(ITypeData concreteType);
    }

    /// <summary>   
    /// A generic type that specifies input properties for a TestStep. The user can link this property to properties on other TestSteps that are marked with the <see cref="OutputAttribute"/>
    /// When used in a TestStep, Input value should always be set in the constructor.
    /// </summary>
    /// <typeparam name="T"> Generic type parameter. </typeparam>
    public class Input<T> : IInput, IInputTypeRestriction, ICloneable
    {
        /// <summary> 
        /// Describes the output property on the <see cref="Step"/> to which this Input is connected.  
        /// </summary>
        [XmlIgnore]
        public IMemberData Property { get; set; }

        void updatePropertyFromName()
        {
            if (string.IsNullOrEmpty(propertyName))
                return;
            else
            {
                string[] parts = propertyName.Split('|');
                if (parts.Length == 1)
                {
                    if (Step != null)
                    {
                        ITypeData stepType = TypeData.GetTypeData(Step);
                        Property = stepType.GetMember(parts[0]);    
                    }
                    else
                    {
                        Property = null;
                    }
                }
                else
                {
                    var typename = parts[0];
                    ITypeData stepType = TypeData.GetTypeData(typename);
                    Property = stepType.GetMember(parts[1]);
                }
            }

            if (Property != null)
                propertyName = null;
        }
        
        string propertyName;
        /// <summary> Gets or sets the name of the property to which this Input is connected. Used for serialization. </summary>
        public string PropertyName
        {
            get => Property != null ? Property.DeclaringType.Name + "|" + Property.Name : propertyName;
            set
            {
                propertyName = value;
                updatePropertyFromName();
                
            }
        }

        Action unbindStep = () => { };

        ITestStep step;
        /// <summary>   Gets or sets the TestStep that has the output property to which this Input is connected. </summary>
        public ITestStep Step 
        {
            get => step;
            set
            {
                if(step != value)
                {
                    step = value;
                    
                    if (step == null)
                    {   
                        return;
                    }
                    stepId = step.Id;
                    unbindStep();
                    List<ITestStepParent> parents = new List<ITestStepParent>();
                    var parent = step.Parent;
                    while(parent != null)
                    {
                        parents.Add(parent);
                        parent.ChildTestSteps.CollectionChanged += ChildTestSteps_CollectionChanged;
                        parent = parent.Parent;
                    }
                    
                    unbindStep = () => parents.ForEach(p => p.ChildTestSteps.CollectionChanged -= ChildTestSteps_CollectionChanged);
                    updatePropertyFromName();
                }
            }
        }

        Guid stepId;
        void ChildTestSteps_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(Step != null && e.OldItems != null && e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems.Contains(step) || Step.GetParents().Any(e.OldItems.Contains))
                {
                    stepId = Step.Id;
                    Step = null;
                }
            }else if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                if(Step != null)
                  stepId = Step.Id;
                Step = null;
            }else  if(e.NewItems != null && e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                var steps = e.NewItems.OfType<ITestStep>();
                var allSteps = Utils.FlattenHeirarchy(steps, x => x.ChildTestSteps).Distinct();
                foreach(var step in allSteps)
                {
                    if(step.Id == stepId)
                    {
                        Step = step;
                        return;
                    }
                }
            }
        }

        /// <summary>   Gets the value of the connected output property. </summary>
        /// <exception cref="Exception">    Thrown when this Input does not contain a reference to a TestStep output. </exception>
        [XmlIgnore]
        public T Value
        {
            get
            {
                if (Step == null || Property == null)
                    throw new Exception("Step input requires reference to a TestStep output.");
                
                // Wait for the step to complete
                return (T)InputOutputRelation.GetOutput(Property, Step); 
            }
        }
        
        /// <summary>Constructor for the Input class.</summary>
        public Input()
        {
            
        }

        /// <summary> Converts the value of this instance to its equivalent string representation. </summary>
        /// <returns> The string representation of the value of this instance. </returns>
        public override string ToString()
        {
            if (Step == null|| Property == null)
                return "";
            var value = Value == null ? "" : StringConvertProvider.GetString(Value);
            return value ?? "";
        }

        /// <summary> Compares one Input to another. </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if(obj is Input<T> other)
                return other.Step == Step && other.Property == Property;
            return false;
        }

        /// <summary> Gets the hash code.</summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (Step?.GetHashCode() ?? 0) ^ (Property?.GetHashCode() ?? 0);
        }

        object ICloneable.Clone() => new Input<T>
        {
            Step = Step,
            Property = Property
        };

        /// <summary> Returns true if this input supports the concrete type. </summary>
        /// <param name="concreteType"></param>
        /// <returns></returns>
        public virtual bool SupportsType(ITypeData concreteType)
        {
            return concreteType.DescendsTo(typeof(T));
        }

        /// <summary> Compares two Input for equality.</summary>
        /// <returns></returns>
        public static bool operator==(Input<T> a, Input<T> b) => object.Equals(a, b);

        /// <summary> Compares two Input for inequality.</summary>
        /// <returns></returns>
        public static bool operator !=(Input<T> a, Input<T> b) => !(a == b);
    }
}
