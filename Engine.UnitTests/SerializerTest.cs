//            Copyright Keysight Technologies 2012-2019
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, you can obtain one at http://mozilla.org/MPL/2.0/.
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using OpenTap.Plugins.BasicSteps;
using OpenTap.Engine.UnitTests.TestTestSteps;
using System.Xml.Linq;

namespace OpenTap.Engine.UnitTests
{
    [TestFixture]
    public class TestPlanTestFixture1
    {
        public class TestPlanTestStep : TestStep
        {
            public class TestPlanTestStep2 : TestStep
            {
                public override void Run()
                {
                }
            }

            public override void Run()
            {
            }
        }

        [Test]
        public void TestPlanSameTestStepNameTest1()
        {
            TestPlan target = new TestPlan();

            target.Steps.Add(new TestPlanTestStep());
            target.Steps.Add(new TestPlanTestStep.TestPlanTestStep2());

            using (var ms = new MemoryStream())
            {
                target.Save(ms);

                Assert.AreNotEqual(0, ms.Length);
            }
        }
    }

    [TestFixture]
    public class TestPlanTestFixture2
    {
        public class TestPlanTestStep : TestStep
        {
            public class TestPlanTestStep2 : TestStep
            {
                public override void Run()
                {
                }
            }

            public override void Run()
            {
            }
        }

        [Test]
        public void TestPlanSameTestStepNameTest2()
        {
            TestPlan target = new TestPlan();

            target.Steps.Add(new TestPlanTestStep());
            target.Steps.Add(new TestPlanTestStep.TestPlanTestStep2());

            using (var ms = new MemoryStream())
            {
                target.Save(ms);
                Assert.AreNotEqual(0, ms.Length);
            }
        }
    }

    [TestFixture]
    public class TestPlanTestFixture3 
    {


        [AllowAnyChild]
        public class TestPlanTestStep : TestStep
        {
            public override void Run()
            {
            }
        }

        [Test]
        public void ChildStepSerialization()
        {
            TestPlan target = new TestPlan();

            ITestStep step = new TestPlanTestStep();
            target.Steps.Add(step);
            step.ChildTestSteps.Add(new TestPlanTestStep());

            using (var ms = new MemoryStream())
            {
                target.Save(ms);

                ms.Seek(0, SeekOrigin.Begin);

                TestPlan deserialized = TestPlan.Load(ms, target.Path);

                Assert.AreEqual(1, deserialized.ChildTestSteps.Count);
                Assert.AreEqual(1, deserialized.ChildTestSteps.First().ChildTestSteps.Count);

                Assert.IsTrue(deserialized.ChildTestSteps.First() is TestPlanTestStep);
                Assert.IsTrue(deserialized.ChildTestSteps.First().ChildTestSteps.First() is TestPlanTestStep);
            }
        }
    }

    [TestFixture]
    public class TestPlanEmptyStringProp 
    {

        public class EmptyStringStep : TestStep
        {
            public string TestProp { get; set; }

            public EmptyStringStep()
            {
                TestProp = "Test";
            }

            public override void Run()
            {
            }
        }

        [Test]
        public void EmptyStringSerialization()
        {
            TestPlan target = new TestPlan();
            var targetStep = new EmptyStringStep { TestProp = "" };

            target.Steps.Add(targetStep);

            using (var ms = new MemoryStream())
            {
                target.Save(ms);

                ms.Seek(0, SeekOrigin.Begin);

                TestPlan deserialized = TestPlan.Load(ms, target.Path);
                var step = deserialized.ChildTestSteps.First() as EmptyStringStep;

                Assert.AreNotEqual(null, step, "Expected step");
                Assert.AreEqual(targetStep.TestProp, step.TestProp);
            }
        }
    }

    [TestFixture]
    public class TestPlanTimespan 
    {
        public class TimespanStep : TestStep
        {
            public TimeSpan TestProp { get; set; }

            public override void Run()
            {
            }
        }

        [Test]
        public void TimespanSerialization()
        {
            TestPlan target = new TestPlan();
            var targetStep = new TimespanStep { TestProp = TimeSpan.FromSeconds(100) };

            target.Steps.Add(targetStep);

            using (var ms = new MemoryStream())
            {
                target.Save(ms);

                ms.Seek(0, SeekOrigin.Begin);

                TestPlan deserialized = TestPlan.Load(ms, null);
                var step = (TimespanStep)deserialized.ChildTestSteps.First();

                Assert.AreNotEqual(null, step, "Expected step");
                Assert.AreEqual(targetStep.TestProp, step.TestProp);
            }
        }
    }

    [TestFixture]
    public class ListSerialization 
    {
        public class StringTemp
        {
            public string Test { get; set; }
        }

        public class StringTempListStep : TestStep
        {
            public List<StringTemp> TestProp { get; set; }
            public System.Collections.ObjectModel.ReadOnlyCollection<string> NullList { get; set; }


            public StringTempListStep()
            {
                TestProp = new List<StringTemp>();
            }

            public override void Run()
            {
            }
        }
        public enum TestEnum
        {
            A, B, C
        }
        public class StringListStep : TestStep
        {

            public List<String> TestProp { get; set; }

            public IList List { get; set; }

            public HashSet<int> Set { get; set; }

            public HashSet<TestEnum> EnumSet { get; set; }

            public Dictionary<string, int> Dict { get; set; }

            public StringListStep()
            {
                TestProp = new List<String>();
                List = new List<double> { 1, 2, 3 };
                Set = new HashSet<int>();
                EnumSet = new HashSet<TestEnum>();
                Dict = new Dictionary<string, int>();
            }

            public override void Run()
            {
            }
        }

        [Test]
        public void StringTempListSerialization()
        {
            TestPlan target = new TestPlan();
            var targetStep = new StringTempListStep { TestProp = new List<StringTemp> { new StringTemp { Test = "123" }, new StringTemp { Test = "abc" } } };

            target.Steps.Add(targetStep);

            using (var ms = new MemoryStream())
            {
                target.Save(ms);

                ms.Seek(0, SeekOrigin.Begin);

                TestPlan deserialized = TestPlan.Load(ms, target.Path);
                var step = deserialized.ChildTestSteps.First() as StringTempListStep;

                Assert.IsNotNull(step.TestProp);
                Assert.AreEqual(targetStep.TestProp.Count, step.TestProp.Count);

                for (int i = 0; i < targetStep.TestProp.Count; i++)
                    Assert.AreEqual(targetStep.TestProp[i].Test, step.TestProp[i].Test);
            }
        }

        [Test]
        public void StringListSerialization()
        {
            TestPlan target = new TestPlan();

            var specList = new List<double> { 5, 6, 7 };
            var hashSet = new HashSet<TestEnum>(new TestEnum[] { TestEnum.C });
            var targetStep = new StringListStep
            {
                TestProp = new List<String> { "123", "abc" },
                List = specList,
                EnumSet = hashSet,
                Set = new HashSet<int>(Enumerable.Range(100, 10)),
                Dict = new Dictionary<string, int>() { }
            };
            targetStep.Dict["asd"] = 5;
            targetStep.Dict[""] = 15;

            target.Steps.Add(targetStep);

            TestPlan deserialized;

            using (var ms = new MemoryStream())
            {
                target.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);
                deserialized = TestPlan.Load(ms, target.Path);
            }
            var step = deserialized.ChildTestSteps.First() as StringListStep;

            Assert.IsNotNull(step.TestProp);
            Assert.AreEqual(targetStep.TestProp.Count, step.TestProp.Count);

            for (int i = 0; i < targetStep.TestProp.Count; i++)
                Assert.AreEqual(targetStep.TestProp[i], step.TestProp[i]);
            Assert.IsTrue(specList.SequenceEqual(step.List.Cast<double>()));
            Assert.IsTrue(step.EnumSet.Except(hashSet).Count() == 0);
            Assert.IsTrue(step.Set.OrderBy(x => x).SequenceEqual(targetStep.Set.OrderBy(x => x)));
            Assert.AreEqual(5, step.Dict["asd"]);
            Assert.AreEqual(15, step.Dict[""]);
        }

        public class InstStep : TestStep
        {
            public List<IInstrument> Instrs { get; set; }

            public override void Run()
            {
            }
        }

        public class SomeotherInstrument : Instrument
        {

        }

        void loadDummyInstruments(int count)
        {
            for (int i = 0; i < count; i++)
            {
                InstrumentSettings.Current.Add(new ScpiDummyInstrument() { Tag = (i + 1).ToString() });
                InstrumentSettings.Current.Add(new SomeotherInstrument());
            }
        }

        void unloadDummyInstruments()
        {
            InstrumentSettings.Current.RemoveIf<IInstrument>(instr => instr is ScpiDummyInstrument && ((ScpiDummyInstrument)instr).Tag != null);
            InstrumentSettings.Current.RemoveIf<IInstrument>(instr => instr is SomeotherInstrument);
        }

        /// <summary>
        /// Tests deserialization/serialzation of a list of instruments.
        /// </summary>
        [Test]
        public void ListInstrumentSerialization()
        {
            try
            {
                loadDummyInstruments(10);
                TestPlan target = new TestPlan();
                Random rnd = new Random(0);
                var targetStep = new InstStep { Instrs = InstrumentSettings.Current.OrderBy(item => rnd.Next()).ToList() };

                target.Steps.Add(targetStep);

                using (var ms = new MemoryStream())
                {
                    target.Save(ms);

                    ms.Seek(0, SeekOrigin.Begin);

                    TestPlan deserialized = TestPlan.Load(ms, target.Path);
                    var step = deserialized.ChildTestSteps.First() as InstStep;

                    Assert.IsNotNull(step.Instrs);
                    Assert.AreEqual(targetStep.Instrs.Count, step.Instrs.Count);

                    for (int i = 0; i < targetStep.Instrs.Count; i++)
                        Assert.AreEqual(targetStep.Instrs[i], step.Instrs[i]);
                }
            }
            finally
            {
                unloadDummyInstruments();
            }
        }

        public class NestedInstStep : TestStep
        {
            public List<List<IInstrument>> Instrs { get; set; }

            public NestedInstStep()
            {
                Instrs = new List<List<IInstrument>>();
            }

            public override void Run()
            {
            }
        }

        /// <summary>
        /// This test step tests serializing/deserializing a list of lists of instruments.
        /// Since Instruments are from ComponentSettingsLists, they should all convert to indexes.
        /// </summary>
        [Test]
        public void ListListInstrumentSerialization()
        {
            loadDummyInstruments(10);
            try
            {
                TestPlan target = new TestPlan();
                Random rnd = new Random(0);
                var randomSeq = InstrumentSettings.Current.OrderBy(item => rnd.Next());
                var targetStep = new NestedInstStep { Instrs = new List<List<IInstrument>> { randomSeq.ToList(), randomSeq.ToList(), randomSeq.ToList() } };

                target.Steps.Add(targetStep);

                using (var ms = new MemoryStream())
                {
                    target.Save(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    TestPlan deserialized = TestPlan.Load(ms, target.Path);
                    var step = deserialized.ChildTestSteps.First() as NestedInstStep;

                    Assert.IsNotNull(step.Instrs);
                    Assert.AreEqual(targetStep.Instrs.Count, step.Instrs.Count);

                    for (int i = 0; i < targetStep.Instrs.Count; i++)
                    {
                        Assert.AreEqual(targetStep.Instrs[i].Count, step.Instrs[i].Count);

                        for (int i2 = 0; i2 < targetStep.Instrs[i].Count; i2++)
                        {
                            Assert.AreEqual(targetStep.Instrs[i][i2], step.Instrs[i][i2]);
                        }
                    }
                }

            }
            finally
            {
                unloadDummyInstruments();
            }
        }

        public class DualNestedInstStep : TestStep
        {
            public List<List<List<IInstrument>>> Instrs { get; set; }

            public DualNestedInstStep()
            {
                Instrs = new List<List<List<IInstrument>>>();
            }

            public override void Run()
            {
            }
        }

        /// <summary>
        /// This test tests serializing / deserializing of lists of lists of lists of instrments.
        /// </summary>
        [Test]
        public void ListListListInstrumentSerialization()
        {
            loadDummyInstruments(10);
            try
            {
                TestPlan target = new TestPlan();
                Random rnd = new Random(0);
                var randomSeq = InstrumentSettings.Current.OrderBy(item => rnd.Next());
                var targetStep = new DualNestedInstStep { Instrs = new List<List<List<IInstrument>>> { new List<List<IInstrument>> { randomSeq.ToList(), randomSeq.ToList() }, new List<List<IInstrument>> { randomSeq.ToList(), randomSeq.ToList(), randomSeq.ToList(), randomSeq.ToList() } } };

                target.Steps.Add(targetStep);

                using (var ms = new MemoryStream())
                {
                    target.Save(ms);

                    ms.Seek(0, SeekOrigin.Begin);

                    TestPlan deserialized = TestPlan.Load(ms, target.Path);
                    var step = deserialized.ChildTestSteps.First() as DualNestedInstStep;

                    Assert.IsNotNull(step.Instrs);
                    Assert.AreEqual(targetStep.Instrs.Count, step.Instrs.Count);

                    for (int i = 0; i < targetStep.Instrs.Count; i++)
                    {
                        Assert.AreEqual(targetStep.Instrs[i].Count, step.Instrs[i].Count);

                        for (int i2 = 0; i2 < targetStep.Instrs[i].Count; i2++)
                        {
                            Assert.AreEqual(targetStep.Instrs[i][i2].Count, step.Instrs[i][i2].Count);

                            for (int i3 = 0; i3 < targetStep.Instrs[i][i2].Count; i3++)
                            {
                                Assert.AreEqual(targetStep.Instrs[i][i2][i3], step.Instrs[i][i2][i3]);
                            }
                        }
                    }
                }

            }
            finally
            {
                unloadDummyInstruments();
            }
        }

        public class DeserializedCallbackTestStep : TestStep, IDeserializedCallback
        {
            public bool WasDeserialized = false;
            public void OnDeserialized()
            {
                WasDeserialized = true;
            }

            public override void Run()
            {

            }
        }

        public class DeserializedCallbackInstrument : Instrument, IDeserializedCallback
        {
            public bool WasDeserialized = false;
            public void OnDeserialized()
            {
                WasDeserialized = true;
            }
        }

        [Browsable(false)]
        public class DeserializedCallbackSettings : ComponentSettings, IDeserializedCallback
        {
            public bool WasDeserialized = false;
            public void OnDeserialized()
            {
                WasDeserialized = true;
            }
        }

        [Test]
        public void TestIDeserializedCallback()
        {
            {// test deserializing plan
                TestPlan plan = new TestPlan();
                DeserializedCallbackTestStep step = new DeserializedCallbackTestStep();
                plan.ChildTestSteps.Add(step);
                using (var tmpFile = new MemoryStream())
                {
                    plan.Save(tmpFile);
                    tmpFile.Position = 0;
                    plan = TestPlan.Load(tmpFile, plan.Path);
                }
                Assert.IsFalse(step.WasDeserialized);
                step = (DeserializedCallbackTestStep)plan.ChildTestSteps[0];
                Assert.IsTrue(step.WasDeserialized);
            }

            bool testSerializeStandaloneInstrument = false;
            if (testSerializeStandaloneInstrument)
            {
                // This cannot work, because there is no ComponentSettings referencing inst.
                // Previously this worked because of an error.

                // test deserializing instrument
                DeserializedCallbackInstrument inst = new DeserializedCallbackInstrument();
                string xml = new TapSerializer().SerializeToString(inst);
                var inst2 = (DeserializedCallbackInstrument)new TapSerializer().DeserializeFromString(xml, TypeData.GetTypeData(inst));
                Assert.IsTrue(inst2.WasDeserialized);
                Assert.IsFalse(inst.WasDeserialized);
            }

            { // test deserializing settings
                DeserializedCallbackSettings settings = new DeserializedCallbackSettings();
                string xml = new TapSerializer().SerializeToString(settings);
                var settings2 = (DeserializedCallbackSettings)new TapSerializer().DeserializeFromString(xml, TypeData.GetTypeData(settings));
                Assert.IsTrue(settings2.WasDeserialized);
                Assert.IsFalse(settings.WasDeserialized);
            }
        }

        class PrivateComponentSettingsList : ComponentSettingsList<PrivateComponentSettingsList, IInstrument>
        {

        }

        [Test]
        public void SerializeDeserializePrivateType()
        {
            var settings = new PrivateComponentSettingsList();
            settings.Add(new ScpiDummyInstrument() { VisaAddress = "test" });
            var xml = new TapSerializer().SerializeToString(settings);

            var settings2 = (PrivateComponentSettingsList)(new TapSerializer().DeserializeFromString(xml, TypeData.FromType(typeof(PrivateComponentSettingsList))));
            var inst = (ScpiDummyInstrument)settings2[0];
            Assert.AreEqual("test", inst.VisaAddress);
        }

        [Test]
        public void SerializeDeserializeNullResource()
        {
            IInstrument instr = new ScpiDummyInstrument();
            InstrumentSettings.Current.Add(instr);
            try
            {
                ScpiTestStep step = new ScpiTestStep();
                step.Instrument = null;
                var xml = new TapSerializer().SerializeToString(step);
                Assert.IsTrue(xml.Contains($"<Instrument />"));
                var deserializedStep = new TapSerializer().DeserializeFromString(xml) as ScpiTestStep;
                Assert.AreEqual(step.Instrument, deserializedStep.Instrument);
            }
            finally
            {
                InstrumentSettings.Current.Remove(instr);
            }
        }

        public class StringObject
        {
            public string TheString { get; set; }
        }

        [Test]
        public void SerializeDeserializeProblematicString()
        {
            {
                StringBuilder sb = new StringBuilder("test:");
                var ser = new TapSerializer();
                for (int i = 0; i < 512; i++)
                    sb.Append((char) i);

                var st = new StringObject() {TheString = sb.ToString()};
                var stt = ser.SerializeToString(st);
                var rev = (StringObject) ser.DeserializeFromString(stt);
                Assert.IsTrue(string.Compare(st.TheString, rev.TheString) == 0);
                
            }
            {
                StringBuilder sb = new StringBuilder("test::::");
                var ser = new TapSerializer();
                for (int i = 0; i < 512; i++)
                   sb.Append((char) i);
                var st = new StringObject() { TheString = sb.ToString() };
                var stt = ser.SerializeToString(st);
                 var rev = (StringObject)ser.DeserializeFromString(stt);
                Assert.IsTrue(string.Compare(st.TheString, rev.TheString) == 0);
                
            }

        }

        public class SimpleClass
        {
            public SimpleClass(object obj)
            {
                
            }
        }
        public class UnbalancedListStep : TestStep
        {
            [Display("Tx Command Sequences", "A predefined list of TX command sequences to control the DUT.",
                Order: 10.2)]
            public IReadOnlyList<SimpleClass> ReadOnlyList { get; set; } = new List<SimpleClass> {new SimpleClass(default), new SimpleClass(default)}.AsReadOnly();

            public override void Run()
            {
                throw new NotImplementedException();
            }
        }
        
        [Test]
        public void DeserializeUnbalancedList()
        {
            using (Session.Create())
            {
                var trace = new EngineUnitTestUtils.TestTraceListener();
                Log.AddListener(trace);

                var plan = new TestPlan();
                plan.ChildTestSteps.Add(new UnbalancedListStep());

                var ser = new TapSerializer();
                var planXml = ser.SerializeToString(plan);
                CollectionAssert.IsEmpty(ser.Errors);

                var currentLength = planXml.Length;

                var itemElem = $"<{nameof(SimpleClass)}></{nameof(SimpleClass)}>";
                // Remove one of the two items from the xml
                planXml = planXml.Remove(planXml.IndexOf(itemElem, StringComparison.Ordinal), itemElem.Length);
                
                // Ensure an element was actually removed
                Assert.AreEqual(currentLength - itemElem.Length, planXml.Length);
                
                var deserializedPlan = ser.DeserializeFromString(planXml) as TestPlan;

                CollectionAssert.IsEmpty(ser.Errors);

                // Deserialization should succeed even though we expect an element which was missing
                Assert.IsTrue(
                    deserializedPlan.ChildTestSteps.First() is UnbalancedListStep s && s.ReadOnlyList.Count == 2,
                    "Expected list to contain 2 element.");
                
                Assert.AreEqual(trace.WarningMessage.Count, 1);
                CollectionAssert.Contains(trace.WarningMessage, "Deserialized unbalanced list.");
            }
        }
    }

    [TestFixture]
    public class SerializeEnumTest
    {

        public class Step1 : TestStep
        {
            public enum SomeEnum { A, B };
            public Instr2 instr1 { get; set; }
            public SomeEnum Value { get; set; }
            public Step1()
            {
                Value = SomeEnum.B;
            }

            public override void Run()
            {
                throw new NotImplementedException();
            }
        }

        public class Step2 : TestStep
        {
            public enum SomeEnum { A, B };

            public SomeEnum Value { get; set; }
            public Step2()
            {
                Value = SomeEnum.B;
            }

            public override void Run()
            {

            }
        }

        public class Instr1 : Instrument
        {
            public enum SomeEnum { A, B };
            public IInstrument Instr { get; set; }
            public SomeEnum Value { get; set; }
            public Instr1()
            {
                Value = SomeEnum.B;
            }
        }

        public class Instr2 : Instrument
        {
            public enum SomeEnum { A, B };
            public SomeEnum Value { get; set; }
            public IInstrument Instr { get; set; }
            public IResultListener Result { get; set; }
            public Instr2()
            {
                Value = SomeEnum.B;
            }
        }

        [DisplayName("Test\\UnitTest Result2")]
        public class Result2 : ResultListener
        {
            public Result2()
            {
                Name = "Result2";
            }

            public IInstrument Instr { get; set; }
        }



        [Test]
        public void TestSerializePlan()
        {
            Step1 step1 = new Step1();
            Step2 step2 = new Step2();
            TestPlan plan1 = new TestPlan();
            plan1.ChildTestSteps.Add(step1);
            plan1.ChildTestSteps.Add(step2);
            string planString;
            using (var ms = new MemoryStream())
            {
                plan1.Save(ms);
                planString = Encoding.UTF8.GetString(ms.ToArray());
            }

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(planString)))
            {
                var plan = TestPlan.Load(ms, plan1.Path);
                Assert.AreEqual(((Step1)plan.ChildTestSteps[0]).Value, Step1.SomeEnum.B);
                Assert.AreEqual(((Step2)plan.ChildTestSteps[1]).Value, Step2.SomeEnum.B);

                // test some overloads.
                ms.Position = 0;
                Assert.IsNotNull(TestPlan.Load(ms, ""));

                ms.Position = 0;
                Assert.IsNotNull(TestPlan.Load(ms, null));
            }
        }

        [Test]
        public void SerializeAndRunSimplePlan()
        {
            var plan = new TestPlan();
            plan.ChildTestSteps.Add(new Step2());
            Assert.AreEqual(Verdict.NotSet, plan.Execute().Verdict);

            using (var ms = new MemoryStream())
            {
                plan.Save(ms);
                ms.Position = 0;

                var plan2 = TestPlan.Load(ms, "");
                var planrun = plan2.Execute();

                Assert.AreEqual(Verdict.NotSet, planrun.Verdict);
            }
        }


        bool membersEquals(object o1, object o2)
        {
            var p1 = o1.GetType().GetPropertiesTap().Where(t => t.PropertyType.IsPrimitive).Select(prop => prop.GetValue(o1, null)).ToList();
            var p2 = o1.GetType().GetPropertiesTap().Where(t => t.PropertyType.IsPrimitive).Select(prop => prop.GetValue(o2, null)).ToList();
            return p1.Zip(p2, object.Equals).All(p => p);
        }

        [Test]
        public void SerializeResourcesWithResources()
        {
            var i1 = new Instr1();
            var i2 = new Instr1();
            InstrumentSettings.Current.Add(i1);
            InstrumentSettings.Current.Add(i2);
            try
            {
                var instruments = new InstrumentSettings();
                instruments.Add(new Instr1() { Value = Instr1.SomeEnum.A, Instr = i2 });
                instruments.Add(new Instr2() { Value = Instr2.SomeEnum.A, Instr = i2 });

                string stringdata;
                using (MemoryStream m = new MemoryStream())
                {
                    using (var x = XmlWriter.Create(m))
                        new TapSerializer().Serialize(x, instruments);
                    stringdata = Encoding.UTF8.GetString(m.ToArray());
                }

                InstrumentSettings instruments2;
                using (MemoryStream m = new MemoryStream(Encoding.UTF8.GetBytes(stringdata)))
                {
                    instruments2 = (InstrumentSettings)new TapSerializer().Deserialize(m);
                }

                Assert.AreEqual(((Instr1)instruments[0]).Value, ((Instr1)instruments2[0]).Value);
                Assert.AreEqual(((Instr2)instruments[1]).Value, ((Instr2)instruments2[1]).Value);
                Assert.AreEqual(((Instr2)instruments[1]).Instr, i2);
                Assert.AreEqual(((Instr1)instruments[0]).Instr, i2);
            }
            finally
            {
                InstrumentSettings.Current.Remove(i1);
                InstrumentSettings.Current.Remove(i2);
            }
        }

        [Test]
        public void SerializeSwitch()
        {
            var trace = new EngineUnitTestUtils.TestTraceListener();
            Log.AddListener(trace);

            // Lets hook up a switch to an RfConnection
            // and check that it can be serialized and deserialized.
            var sw = new DummySwitchIntrument();
            var rfCon = new RfConnection();
            rfCon.Via.Add(sw.Positions[0]);
            rfCon.Via.Add(sw.Position1);

            InstrumentSettings.Current.Clear();
            InstrumentSettings.Current.Add(sw);
            ConnectionSettings.Current.Clear();
            ConnectionSettings.Current.Add(rfCon);

            InstrumentSettings.Current.Save();
            ConnectionSettings.Current.Save();

            // Ensure that we are actually getting serialize/deserialized.
            InstrumentSettings.Current.Clear();
            ConnectionSettings.Current.Clear();

            // Ask to deserialize.
            InstrumentSettings.Current.Invalidate();
            ConnectionSettings.Current.Invalidate();

            sw = InstrumentSettings.Current[0] as DummySwitchIntrument;
            rfCon = ConnectionSettings.Current[0] as RfConnection;
            Assert.AreEqual(rfCon.Via[0], sw.Positions[0]);
            Assert.AreEqual(rfCon.Via[1], sw.Position1);


            Log.Flush();
            Log.RemoveListener(trace);
            trace.AssertErrors();
        }

        [Test]
        public void SerializeResourcesWithResourcesTough()
        {
            // Tough because instruments contains references between them
            var i1 = new Instr1();
            var i2 = new Instr2();
            i1.Instr = i2;
            i2.Instr = i1;
            var orig = InstrumentSettings.GetSettingsDirectory("Bench");
            InstrumentSettings.SetSettingsProfile("Bench", orig + "InstrumentTestDir");
            InstrumentSettings.Current.Add(i1);
            InstrumentSettings.Current.Add(i2);
            try
            {
                InstrumentSettings.Current.Save();

                InstrumentSettings.SetSettingsProfile("Bench", orig);
                InstrumentSettings.Current.ToString();
                InstrumentSettings.SetSettingsProfile("Bench", orig + "InstrumentTestDir");
                Assert.IsTrue(InstrumentSettings.Current.OfType<Instr1>().Any());
                var arr = InstrumentSettings.Current.Cast<dynamic>().ToArray();
                Assert.AreEqual(arr[0], arr[1].Instr);
                Assert.AreEqual(arr[1], arr[0].Instr);
            }
            finally
            {
                InstrumentSettings.SetSettingsProfile("Bench", orig);
                Directory.Delete(orig + "InstrumentTestDir", true);
            }
        }

        [Test]
        public void SerializePlatformSettings()
        {
            var toSerialize = EngineSettings.Current;
            var prevStationName = toSerialize.StationName;
            toSerialize.StationName = "hello";
            string stringdata;
            using (MemoryStream m = new MemoryStream())
            {
                using (var x = XmlWriter.Create(m))
                    new TapSerializer().Serialize(x, toSerialize);
                stringdata = Encoding.UTF8.GetString(m.ToArray());
            }

            EngineSettings platformSettings;
            using (MemoryStream m = new MemoryStream(Encoding.UTF8.GetBytes(stringdata)))
                platformSettings = (EngineSettings)new TapSerializer().Deserialize(m);
            Assert.IsTrue(membersEquals(toSerialize, platformSettings));
        }

        [Test]
        public void DeserializeLegacyPlatformSettings()
        {
            EngineSettings settings = (EngineSettings)new TapSerializer().DeserializeFromString(Resources.GetEmbedded("LegacyPlatformSettings.xml"), TypeData.FromType(typeof(EngineSettings)));
            Assert.AreEqual(settings.OperatorName, "SomeOperator");
        }

        [Test, Ignore("We are not backwards compatible.")]
        public void DeserializeLegacyResultSettings()
        {
            ResultSettings settings = (ResultSettings)new TapSerializer().DeserializeFromString(Resources.GetEmbedded("LegacyResultSettings.xml"), TypeData.FromType(typeof(ResultSettings)));
            Assert.IsTrue(settings[0] is LogResultListener);
            Assert.IsTrue((settings[0] as LogResultListener).FilePath == "SomePath.txt");
            Assert.IsTrue((settings[0] as LogResultListener).FilterOptions ==
                (LogResultListener.FilterOptionsType.Errors | LogResultListener.FilterOptionsType.Verbose | LogResultListener.FilterOptionsType.Info));
            var serialized = new TapSerializer().SerializeToString(settings);
            var deserialized = (ResultSettings)new TapSerializer().DeserializeFromString(serialized);
            Assert.IsTrue((settings[0] as LogResultListener).FilePath == (deserialized[0] as LogResultListener).FilePath);
        }

        [Test]
        public void DeserializeLegacyTapPlan()
        {
            var stream = File.OpenRead("TestTestPlans/FiveDelays.TapPlan");
            TestPlan plan = (TestPlan)new TapSerializer().Deserialize(stream, type: TypeData.FromType(typeof(TestPlan)));

            var childSteps = plan.ChildTestSteps.OfType<DelayStep>().ToArray();
            Assert.AreEqual(childSteps.Length, 5);
        }

        [Test]
        [Retry(10)]
        // [Repeat(1000)] // TODO: this test sometimes fails due to a timing issue. Enable the repeat to reproduce.
        public void RunMultipleTestPlanReferences()
        {
            PlanRunCollectorListener pl = new PlanRunCollectorListener();
            ResultSettings.Current.Add(pl);
            try
            {
                var ser = new TapSerializer();

                ser.GetSerializer<Plugins.ExternalParameterSerializer>()
                    .PreloadedValues["path1"] = "TestTestPlans/testReferencedPlan3.TapPlan";
                TestPlan plan = (TestPlan)ser.DeserializeFromFile("TestTestPlans/testMultiReferencePlan.TapPlan");

                // The last step is a SetVerdict step, which points into the last TestPlanReferenceStep.
                // inside that there should be a SetVerdict that results in an inconclusive verdict.
                var run = plan.Execute();
                Assert.AreEqual(13, run.StepsWithPrePlanRun.Count);
                Assert.AreEqual(13, pl.StepRuns.Count);
                Assert.AreEqual(Verdict.Inconclusive, run.Verdict);
                Assert.AreEqual(Verdict.Pass, pl.StepRuns[12].Verdict);
            }
            finally
            {
                ResultSettings.Current.Remove(pl);
            }
        }
        [Test]
        [Ignore("support for 7.x test plans is dropped.")]
        public void LoadLegacyTestPlanReference()
        {
            var ser = new TapSerializer();
            TestPlan plan = (TestPlan)ser.DeserializeFromFile("TestTestPlans\\LegacyRefPlan.TapPlan");
            Assert.AreEqual(4, plan.ChildTestSteps.Count,"Unexpected number of childsteps.");
            Assert.IsTrue(plan.ChildTestSteps[0].ChildTestSteps[0] is VerdictStep);
            Assert.IsTrue(plan.ChildTestSteps[1].ChildTestSteps[0] is VerdictStep);
            Assert.IsTrue(plan.ChildTestSteps[2].ChildTestSteps[0] is VerdictStep);
            Assert.IsTrue(plan.ChildTestSteps[3].ChildTestSteps[0] is VerdictStep);

        }

    }

    public class DynamicStepTest : TestStep, IDynamicStep
    {
        public string NewData { get; set; }

        [XmlIgnore]
        public string NewData2 { get; set; }


        public DynamicStepTest()
        {
            NewData = "Hello";
        }

        public DynamicStepTest(string data) : this()
        {
            NewData2 = NewData + data;
        }


        public Type GetStepFactoryType()
        {
            return typeof(DynamicStepTest);
        }

        public ITestStep GetStep()
        {
            return new DynamicStepTest(NewData);
        }

        public override void Run()
        {
            throw new NotImplementedException();
        }
    }
    [TestFixture]
    public class DynamicStepSerialization
    {
        [Test]
        public void SerializeDeserializeDynamicStep()
        {
            DynamicStepTest step = new DynamicStepTest();
            TestPlan plan = new TestPlan();
            plan.ChildTestSteps.Add(step);

            TestPlan loaded = null;
            using (var memstr = new MemoryStream())
            {
                plan.Save(memstr);
                memstr.Seek(0, SeekOrigin.Begin);
                var str = Encoding.UTF8.GetString(memstr.ToArray());
                loaded = TestPlan.Load(memstr, plan.Path);
            }
            Assert.AreEqual("HelloHello", ((DynamicStepTest)loaded.ChildTestSteps[0]).NewData2);
        }
        [Test]
        public void SerializeDeserializeDynamicStepCompress()
        {
            DynamicStepTest step = new DynamicStepTest();

            TestPlan plan = new TestPlan();
            plan.ChildTestSteps.Add(step);

            var NewDataSav = step.NewData + "Hello";  //save before compress
            TestPlan loaded = null;
            using (var memstr = new MemoryStream())
            {
                plan.Save(memstr);
                memstr.Seek(0, SeekOrigin.Begin);
                var str = Encoding.UTF8.GetString(memstr.ToArray());
                loaded = TestPlan.Load(memstr, plan.Path);
            }
            Assert.AreEqual(NewDataSav, ((DynamicStepTest)loaded.ChildTestSteps[0]).NewData2);
        }

        [Test]
        public void ParseNumber()
        {
            var parser = new NumberFormatter(CultureInfo.InvariantCulture);
            var lst = parser.Parse("4:1,4:1,4:1");
            Assert.AreEqual(30, lst.CastTo<int>().Sum());
            Assert.AreEqual(0, parser.Parse("-30, 1:4, 1:4, 1:4").CastTo<int>().Sum());
            var r1 = parser.Parse("-30, 1:4, 1:4, 3:6");
            var rb = parser.FormatRange(r1);
            double v1 = r1.CastTo<int>().Sum();
            Assert.AreEqual(v1, 8);
            r1 = parser.Parse(rb);
            double v2 = r1.CastTo<int>().Sum();
            Assert.AreEqual(v1, v2);
            var rb2 = parser.FormatRange(r1);
            Assert.AreEqual(rb, rb2);
            parser.Unit = "Hz";

            var tst = new BigFloat(1e12);

            var number1 = (double)parser.ParseNumber("1e6 kHz", typeof(double));
            var number2 = (double)parser.ParseNumber("1e3 MHz", typeof(double));
            var number3 = (double)parser.ParseNumber("1.0 GHz", typeof(double));

            Assert.IsTrue(Math.Abs(number1 - 1e9) == 0);
            Assert.IsTrue(Math.Abs(number2 - 1e9) == 0);
            Assert.IsTrue(Math.Abs(number3 - 1e9) == 0);


            var rb_unit = parser.FormatRange(r1);

            Assert.AreEqual(30, parser.Parse("4 Hz:1 Hz,4:1 Hz,4 Hz:1").CastTo<int>().Sum());
            Assert.AreEqual(30000, parser.Parse("4 kHz:-1k:1 kHz,4 k:-1k:1 kHz,4 kHz:-1k:1k").CastTo<int>().Sum());
            parser.UsePrefix = true;
            Assert.AreEqual("-2.5 kHz", parser.FormatRange(new decimal[] { -2500 }));
            //Assert.AreEqual(2, parser.Parse("0:1/3:1").Sum());
            Assert.AreEqual(0x10, (int)parser.Parse("0x10").CastTo<int>().First());
            Assert.AreEqual(0xFF, (int)parser.Parse("0xFF").CastTo<int>().First());
            Assert.AreEqual(0xff, (int)parser.Parse("0xff").CastTo<int>().First());
            Assert.AreEqual(0x11, (int)parser.Parse("0b00010001").CastTo<int>().First());
            parser.PreScaling = 1000;
            var val = parser.ParseNumber("0.001", typeof(int));
            var val_inv = parser.FormatNumber(val);
            var val2 = parser.ParseNumber(val_inv, typeof(int));

            parser.PreScaling = 1;
            parser.Format = "X2";
            parser.Unit = "";
            parser.UsePrefix = false;
            Assert.AreEqual(0x10, (int)parser.Parse("10").CastTo<int>().First());
            Assert.AreEqual(0xFF, (int)parser.Parse("FF").CastTo<int>().First());
            Assert.AreEqual(0xFF, (int)parser.Parse("ff").CastTo<int>().First());
            Assert.AreEqual(-0xFF, (int)parser.Parse("-FF").CastTo<int>().First());
            Assert.AreEqual(-0xFF, (int)parser.Parse("-ff").CastTo<int>().First());

            Assert.AreEqual("10", parser.FormatNumber(16));
            Assert.AreEqual("FF", parser.FormatNumber(255));

            parser.Format = "";
            Assert.AreEqual(15, parser.Parse("0,1,2,3,4,5").CastTo<int>().Sum());
            Assert.AreEqual(6, parser.Parse("0,1,2,3,4,5").CastTo<int>().Count());
            Assert.AreEqual("0 : 5", parser.FormatRange(parser.Parse("0,1,2,3,4,5")));
            parser.UseRanges = false;
            Assert.AreEqual("1, 2, 3, 4, 5", parser.FormatRange(parser.Parse("1,2,3,4,5")));
            Assert.AreEqual("1, 2, 3, 4, 5", parser.FormatRange(parser.Parse("1 : 5")));


            var conv = TypeDescriptor.GetConverter(new BigFloat(2));
            var valx1 = (BigFloat)conv.ConvertFrom(1.0);
            double valx = (double)conv.ConvertTo(valx1, typeof(double));
            Assert.AreEqual(1.0, valx);

            { // 3085
                var parser2 = new NumberFormatter(CultureInfo.InvariantCulture) { Format = "N3", UsePrefix = true, Unit = "V" };
                var parsed = (double)parser2.ParseNumber("2.5 V", typeof(double));
                var revparsed = parser2.FormatNumber(parsed);
                Assert.AreEqual(2.5, parsed);
                Assert.AreEqual("2.500 V", revparsed);

                parser2.Format = "R";
                revparsed = parser2.FormatNumber(parsed);
                Assert.AreEqual("2.5 V", revparsed);
            }

            { // bug: #3087 issue with List<int> of value {255,255,0,0}
                parser.Unit = null;
                parser.UseRanges = true; // essential
                var result = parser.Parse("255, 255,  0").CastTo(typeof(int)).OfType<int>().ToList();
                var result2 = parser.FormatRange(result);
                var result3 = parser.Parse(result2).CastTo(typeof(int)).OfType<int>().ToList();
                Assert.IsTrue(result.SequenceEqual(new int[] { 255, 255, 0 }));
                Assert.IsTrue(result3.SequenceEqual(new int[] { 255, 255, 0 }));
            }

            {
                int x = -1000000;
                var bf = new BigFloat(x);
                bf.ToString();
                int y = (int)bf.ConvertTo(typeof(int));
                Assert.AreEqual(x, y);
            }

        }

        [Test]
        public void BigFloatTest()
        {
            int[] num = new[] { 1, -1, 1, -1, 10, -100, 21464000 };
            int[] dnum = new[] { 2, 3, 4, 5, 100, 1000, 1 };
            string[] results = new[] { "0.5", "-0.333333333", "0.25", "-0.2", "0.1", "-0.1", "21464000" };
            foreach (var r in results)
            {
                var x = BigFloat.Convert(r, CultureInfo.InvariantCulture);
                Assert.AreEqual(r, x.ToString(CultureInfo.InvariantCulture));

                var y = BigFloat.Convert(r, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                Assert.AreEqual(r, y);
            }

            BigFloat bf = new BigFloat(0.2002002 - 0.00019999999999999993);
            var s1 = bf.ToString(CultureInfo.InvariantCulture);
            StringAssert.StartsWith("0.2", s1);

            bf = new BigFloat(-200000000001);
            var s6 = bf.ToString(CultureInfo.InvariantCulture);
            Assert.AreEqual("-200000000001", s6);

            var parsed = BigFloat.Convert("1.00001", CultureInfo.InvariantCulture);
            BigFloat pi = new BigFloat(Math.PI);
            var stringv = (pi + pi).ToString(CultureInfo.InvariantCulture);

            var parsed2 = BigFloat.Convert("1.0000", CultureInfo.InvariantCulture);
            var stringv2 = parsed2.ToString(CultureInfo.InvariantCulture);

            var rng = new Range(0, 10);
            Assert.IsTrue(rng.Count() == 11);
            var rng2 = new Range(0, 10.1, 0.5);
            Assert.AreEqual(21, rng2.Count());

            Assert.IsTrue(new BigFloat(1) > new BigFloat(0.1));
            Assert.IsTrue(new BigFloat(0.1) > new BigFloat(0.01));
            Assert.IsTrue(new BigFloat(0.01) > new BigFloat(0.001));
            Assert.IsTrue(new BigFloat(0.001) > new BigFloat(0.0));

            var a = new BigFloat(1) * (1 + ((new BigFloat(1) - new BigFloat(0)) / new BigFloat(1)).Round());
            var b = new BigFloat(1) * (((new BigFloat(1) - new BigFloat(0)) / new BigFloat(1)).Round());
            Assert.IsFalse(a == b);

            var test1 = new BigFloat("0.031113876", CultureInfo.InvariantCulture).Normalize();

            var number = new BigFloat("10000000000000000000000000000000000000000000000000000.00000000000000000000000000000001", CultureInfo.InvariantCulture);
            var number2 = number * 2;

            var tests = new[] { new BigFloat(3000001, 10000), new BigFloat(1, 100000), new BigFloat(0, 10), new BigFloat(-100, 3214), new BigFloat(1, 3) };
            foreach (var test in tests)
            {
                var str = test.ToString();
                var test2 = BigFloat.Convert(str);
                //Assert.AreEqual(test, test2);
            }

            {   // test bug #3075: float values converted to double when displayed (this caused rounding error). 
                float floattest = 0.05f;
                var floattest_big = new BigFloat(floattest);
                float floattest_result = (float)floattest_big.ConvertTo(typeof(float));
                Assert.AreEqual(floattest, floattest_result);
                var f1_str = floattest.ToString();
                var f2_str = floattest_big.ToString();
                Assert.AreEqual(f1_str, f2_str);
            }

        }

        [Test]
        public void HugeBigFloatTest()
        {
            // very huge numbers are not supported since they take a lot of time to parse.
            Assert.Throws<FormatException>(() => new BigFloat("1e99999999")); // too huge.
            Assert.Throws<FormatException>(() => new BigFloat("1e-99999999")); // too precise.
            Assert.DoesNotThrow(() => new BigFloat("1e999")); // huge but ok.
            Assert.DoesNotThrow(() => new BigFloat("1e-999")); // really precise but ok.
        }

        private static bool Test<T>(T a, T b, Func<T, T, bool> test) { return test(a, b); }
        private static T Calc<T>(T a, T b, Func<T, T, T> test) { return test(a, b); }

        [Test]
        public void BigFloatComparisons()
        {
            var values = new[] { 0, double.NaN, double.NegativeInfinity, double.PositiveInfinity };
            var bigValues = new[] { 0, BigFloat.NaN, BigFloat.NegativeInfinity, BigFloat.Infinity };

            for (int i = 0; i < values.Length; i++)
                for (int i2 = 0; i2 < values.Length; i2++)
                {
                    Assert.AreEqual(Test(values[i], values[i2], (a, b) => a == b), Test(bigValues[i], bigValues[i2], (a, b) => a == b), "{0} == {1}", values[i], values[i2]);
                    Assert.AreEqual(Test(values[i], values[i2], (a, b) => a < b), Test(bigValues[i], bigValues[i2], (a, b) => a < b), "{0} < {1}", values[i], values[i2]);
                    Assert.AreEqual(Test(values[i], values[i2], (a, b) => a > b), Test(bigValues[i], bigValues[i2], (a, b) => a > b), "{0} > {1}", values[i], values[i2]);
                    Assert.AreEqual(Test(values[i], values[i2], (a, b) => a != b), Test(bigValues[i], bigValues[i2], (a, b) => a != b), "{0} != {1}", values[i], values[i2]);
                    Assert.AreEqual(Test(values[i], values[i2], (a, b) => a <= b), Test(bigValues[i], bigValues[i2], (a, b) => a <= b), "{0} <= {1}", values[i], values[i2]);
                    Assert.AreEqual(Test(values[i], values[i2], (a, b) => a >= b), Test(bigValues[i], bigValues[i2], (a, b) => a >= b), "{0} >= {1}", values[i], values[i2]);
                }
        }

        [Test]
        public void BigFloatOps()
        {
            var values = new[] { 0, 1, -1, 2, -2, double.NaN, double.NegativeInfinity, double.PositiveInfinity };
            var bigValues = new[] { 0, 1, -1, 2, -2, BigFloat.NaN, BigFloat.NegativeInfinity, BigFloat.Infinity };

            for (int i = 0; i < values.Length; i++)
            {
                // Bumping to .NET6 has brought some slight behavior change to double.ToString().
                // (-0).ToString() now returns "-0" instead of "0". This test has been modified
                // to ensure we retain the old BigFloat tostring behavior.
                string toString(double d)
                {
                    var str = d.ToString();
                    if (str == "-0") return "0";
                    return str;
                }

                Assert.AreEqual(toString(-values[i]), (-bigValues[i]).ToString(), "-{0}", values[i]);
                Assert.AreEqual(toString(Math.Abs(values[i])), bigValues[i].Abs().ToString(), "abs({0})", values[i]);
                Assert.AreEqual(toString(Math.Round(values[i])), bigValues[i].Round().ToString(), "round({0})", values[i]);
                Assert.AreEqual((int)values[i], (int)bigValues[i], "(int){0}", values[i]);

                for (int i2 = 0; i2 < values.Length; i2++)
                {
                    Assert.AreEqual(toString(Calc(values[i], values[i2], (a, b) => a + b)), Calc(bigValues[i], bigValues[i2], (a, b) => a + b).ToString(), "{0} + {1}", values[i], values[i2]);
                    Assert.AreEqual(toString(Calc(values[i], values[i2], (a, b) => a - b)), Calc(bigValues[i], bigValues[i2], (a, b) => a - b).ToString(), "{0} - {1}", values[i], values[i2]);
                    Assert.AreEqual(toString(Calc(values[i], values[i2], (a, b) => a * b)), Calc(bigValues[i], bigValues[i2], (a, b) => a * b).ToString(), "{0} * {1}", values[i], values[i2]);
                    Assert.AreEqual(toString(Calc(values[i], values[i2], (a, b) => a / b)), Calc(bigValues[i], bigValues[i2], (a, b) => a / b).ToString(), "{0} / {1}", values[i], values[i2]);
                }
            }
        }

        [Test]
        public void ParseRangesPreceisely()
        {
            var nf = new NumberFormatter(System.Globalization.CultureInfo.InvariantCulture);

            Action<string, int> TestRange = (s, cnt) =>
            {
                var values = nf.Parse(s).CastTo<double>().ToArray();
                Assert.AreEqual(cnt, values.Length, "Number of parsed elements");
                var rng = nf.FormatRange(values);
                var newValues = nf.Parse(rng).CastTo<double>().ToArray();
                CollectionAssert.AreEqual(values, newValues, "Array formatting failed");

                var sequence = nf.Parse(s);
                var sequenceValues = sequence.CastTo<double>().ToArray();
                var newSequenceValues = nf.Parse(nf.FormatRange(sequence)).CastTo<double>().ToArray();
                CollectionAssert.AreEqual(sequenceValues, newSequenceValues, "Combined sequence formatting failed");
            };

            TestRange("0.1e-10, 1, 2, 3", 4);
            TestRange("1,2,3.5,5", 4);
            TestRange("1 : 1 : 2.9, 6", 3);
            TestRange("1 : 1 : 3.1, 6", 4);
            TestRange("21464000", 1);

            // These numbers can occur when input in the GUI for example. The first is very imprecisely handled.
            TestRange("2.1464 G, 2.1473 G, 2.1482 G, 2127000000, 2128000000, 2128800000, 2129700000, 2130600000, 2131500000, 2132400000, 2147700000 : 900000 : 2153100000, 2122800000, 2123700000, 2124600000, 2125500000, 2126400000, 2127300000, 2128200000, 2129100000, 2130000000, 2150000000 : 900000 : 2157200000", 35);

            Assert.That(() => new Range(1.0, 2.0, 0), Throws.TypeOf<Exception>());


        }

        public class DynStep : TestStep, IDynamicStep
        {
            public ITestStep GetStep()
            {
                return new DynStep();
            }

            public Type GetStepFactoryType()
            {
                return typeof(DynStep);
            }

            public override void Run()
            {
            }
        }

        [Test]
        public void SerializerDynamicStepAttributeTets()
        {
            DynStep st = new DynStep();
            st.Id = Guid.NewGuid();

            TestPlan tp = new TestPlan();
            tp.ChildTestSteps.Add(st);

            TestPlan tp2 = null;

            using (var memstr = new MemoryStream())
            {
                tp.Save(memstr);
                memstr.Seek(0, SeekOrigin.Begin);
                tp2 = TestPlan.Load(memstr, tp.Path);
            }

            Assert.AreEqual(st.Id, tp2.ChildTestSteps.First().Id);
        }

        public enum SerTestEnum
        {
            A,
            B
        }

        public class SerializedStep<T> : TestStep
        {
            public T[] Prop { get; set; }

            public override void Run()
            {
            }
        }

        [Test]
        public void SerializerEnumArray()
        {
            var sw1 = new SweepLoop();
            var sw2 = new SweepLoop();
            var ds = new DelayStep();

            var tp = new TestPlan();
            tp.ChildTestSteps.Add(sw1);
            sw1.ChildTestSteps.Add(sw2);
            sw2.ChildTestSteps.Add(ds);

            sw1.SweepParameters.Add(new SweepParam(new IMemberData[] { TypeData.GetTypeData(sw2).GetMember("CrossPlan") }, SweepLoop.SweepBehaviour.Across_Runs, SweepLoop.SweepBehaviour.Within_Run));

            var str = new TapSerializer().SerializeToString(tp);
            var tp2 = (TestPlan)new TapSerializer().DeserializeFromString(str);

            Assert.IsNotNull(tp2);
            Assert.AreEqual(1, tp2.ChildTestSteps.Count);

            var beh2 = (SweepLoop)tp.ChildTestSteps[0];

            Assert.IsNotNull(beh2);
            /*Assert.IsNotNull(beh2.Prop);
            Assert.AreEqual(2, beh2.Prop.Length);
            Assert.AreEqual(SerTestEnum.A, beh2.Prop[0]);
            Assert.AreEqual(SerTestEnum.B, beh2.Prop[1]);*/
        }

        public class ListTestInstr : Instrument
        {
            [XmlIgnore]
            public Port P1 { get; set; }
            [XmlIgnore]
            public Port P2 { get; set; }

            [XmlIgnore]
            public List<Port> Ports { get; set; }

            public ListTestInstr()
            {
                P1 = new Port(this, "Port1");
                P2 = new Port(this, "Port2");
                Ports = new List<Port> { new Port(this, "Port3"), new Port(this, "Port4") };
            }
        }

        public class ListStep : TestStep, IDeserializedCallback
        {
            public double SomeNumber { get; set; }
            public Port TestPort { get; set; }

            public IEnumerable<double> Numbers { get; set; }
            public List<decimal> Decimals { get; set; }
            public double[] Doubles { get; set; }

            public byte[] Bytes { get; set; }

            public List<bool> Booleans { get; set; }

            public Guid Id2 { get; set; }

            public TimeSpan TimeSpan { get; set; }

            public DateTime DateTime { get; set; } // kind of supported, but only down to sec precision.

            [XmlIgnore] // not supported.
            public Type Type { get; set; }

            public System.Security.SecureString SecureString { get; set; }

            public ListTestInstr Instrument { get; set; }

            public System.Collections.ObjectModel.ReadOnlyCollection<int> ReadOnly { get; set; }

            public Enabled<ListTestInstr> Instrument2 { get; set; }
            public Enabled<TimeSpan> TimeSpan2 { get; set; }
            public string TestString1 { get; set; }

            public string TestString2 { get; set; }

            public string TestString3 { get; set; }

            [XmlIgnore]
            public string NullThing { get; set; }

            public ListStep OtherStep { get; set; }

            public bool ABool { get; set; }

            public List<string> ListOfStrings { get; set; }

            public List<Verdict> ListOfVerdicts { get; set; } = new List<Verdict> { Verdict.Pass };

            public double? NullableNumber { get; set; } = 5;
            public override void Run()
            {
                throw new NotImplementedException();
            }

            public ListStep()
            {

            }

            public void Load()
            {
                Numbers = Enumerable.Range(4, 10).Select(x => (double)x).ToArray();
                Decimals = new List<decimal>() { 1, 2, 3, 4, 5 };
                Bytes = new byte[] { 9, 100, 1, 2, 3, 4, 5, 6 };
                Id2 = new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11);
                TimeSpan = TimeSpan.FromSeconds(1.5);
                DateTime = new DateTime(2017, 1, 1);
                Type = typeof(DateTime);
                SecureString = "asd".ToSecureString();
                Instrument = InstrumentSettings.Current.OfType<ListTestInstr>().Skip(1).FirstOrDefault();
                TestPort = Instrument.P1;
                ReadOnly = Enumerable.Range(10, 3).ToList().AsReadOnly();
                Instrument2 = new Enabled<ListTestInstr>() { Value = Instrument };
                TimeSpan2 = new Enabled<TimeSpan>() { Value = TimeSpan, IsEnabled = true };
                TestString1 = "hello";
                TestString2 = null;
                TestString3 = "\0\0\0";
                NullThing = "asd";
                SomeNumber = 3.14;
                ABool = true;
                Booleans = new List<bool> { true, false, true, false };
                ListOfStrings = new List<string> { "asd", "dsa" };
                Doubles = new double[] { 1, 2, 3, 4 };
                ListOfVerdicts = new List<Verdict> { Verdict.Pass, Verdict.Fail };
            }

            public void CheckSame(ListStep step)
            {
                Assert.AreEqual(step.ABool, ABool);
                Assert.IsTrue(step.Numbers.SequenceEqual(Numbers));
                Assert.IsTrue(step.Decimals.SequenceEqual(Decimals));
                Assert.IsTrue(step.Bytes.SequenceEqual(Bytes));
                Assert.IsTrue(step.Booleans.SequenceEqual(Booleans));
                Assert.IsTrue(step.ReadOnly.SequenceEqual(ReadOnly));
                Assert.IsTrue(step.ListOfStrings.SequenceEqual(ListOfStrings));
                Assert.IsTrue(step.ListOfVerdicts.SequenceEqual(ListOfVerdicts));
                Assert.AreEqual(step.Id2, Id2);
                Assert.AreEqual(step.DateTime, DateTime);
                //Assert.AreEqual(step.Type, Type);
                Assert.AreEqual(step.SecureString.ConvertToUnsecureString(), SecureString.ConvertToUnsecureString());
                Assert.AreEqual(step.Instrument, Instrument);
                Assert.AreEqual(step.TestPort, TestPort);
                Assert.AreEqual(step.Instrument2.IsEnabled, Instrument2.IsEnabled);
                Assert.AreEqual(step.Instrument2.Value, Instrument2.Value);
                Assert.AreEqual(step.Instrument, Instrument2.Value);
                Assert.AreEqual(step.TimeSpan2.IsEnabled, TimeSpan2.IsEnabled);
                Assert.AreEqual(step.TimeSpan2.Value, TimeSpan2.Value);
                Assert.AreEqual(step.TestString1, TestString1);

                Assert.AreEqual(step.SomeNumber, SomeNumber);
                Assert.AreEqual(step.Name, Name);
                Assert.IsTrue(step.Doubles.SequenceEqual(Doubles));

                // Null is not supported for strings an empty string is used instead.
                //Assert.AreEqual(step.TestString2, TestString2); 
                Assert.AreEqual(step.TestString1, TestString1);
            }

            public bool WasDeserialized = false;

            public void OnDeserialized()
            {
                WasDeserialized = true;
            }
        }

        public class StepWithDuplicateProperty : TestStep
        {
#pragma warning disable CS0108
            public string Results { get; set; } // Results already exists in TestStep.
#pragma warning restore CS0108
            public override void Run()
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void DuplicateProperty()
        {
            var tp = new TestPlan();
            tp.ChildTestSteps.Add(new StepWithDuplicateProperty());
            using (var memstr = new MemoryStream())
            {
                tp.Save(memstr);
                memstr.Position = 0;
                tp = TestPlan.Load(memstr, ".");
            }
            Assert.IsTrue(tp.ChildTestSteps.Count == 1);
        }


        [Test]
        public void SerializeFloat()
        {
            var step = new DelayStep();
            step.DelaySecs = 0.6822871999174;

            var tp = new TestPlan();
            tp.ChildTestSteps.Add(step);

            using (var ms = new MemoryStream())
            {
                tp.Save(ms);
                ms.Seek(0, 0);
                var tp2 = TestPlan.Load(ms, "");

                Assert.AreEqual(step.DelaySecs, (tp2.ChildTestSteps[0] as DelayStep).DelaySecs, "Delay value");
            }
        }

        [Test]
        public void SerializerNumberList()
        {
            InstrumentSettings.Current.Add(new ListTestInstr());
            InstrumentSettings.Current.Add(new ListTestInstr());
            try
            {
                ListStep step1 = new ListStep();
                ListStep step3 = new ListStep();


                var someNumberProp = TypeData.FromType(typeof(ListStep)).GetMember("SomeNumber");
                var doublesProp = TypeData.FromType(typeof(ListStep)).GetMember("Doubles");

                step1.Load();
                step3.Load();
                step1.OtherStep = step3;
                step3.OtherStep = step1;
                TestPlan plan = new TestPlan();
                plan.ChildTestSteps.Add(step1);
                plan.ChildTestSteps.Add(step3);
                plan.ExternalParameters.Add(step1, someNumberProp, "SomeNumber");
                plan.ExternalParameters.Add(step1, doublesProp, "Doubles");
                
                TestPlan plan2 = null;
                using (var memstr = new MemoryStream())
                {
                    plan.Save(memstr);
                    var str = Encoding.UTF8.GetString(memstr.ToArray());
                    memstr.Position = 0;
                    plan2 = TestPlan.Load(memstr, plan.Path);
                }
                var step2 = (ListStep)plan2.ChildTestSteps[0];
                var step4 = (ListStep)plan2.ChildTestSteps[1];
                step2.CheckSame(step1);
                step4.CheckSame(step3);
                Assert.AreNotEqual(step2, step4);
                Assert.AreEqual(step2.OtherStep, step4);
                Assert.AreEqual(step4.OtherStep, step2);

                Assert.IsTrue(step2.WasDeserialized);
                Assert.IsNull(step2.NullThing);
                var ext = plan2.ExternalParameters.Get("SomeNumber");
                Assert.IsNotNull(ext);
                Assert.AreEqual((double)ext.Value, step1.SomeNumber);

            }
            finally
            {
                InstrumentSettings.Current.RemoveIf<IInstrument>(x => x is ListTestInstr);
            }
        }

        public class RefDut : Dut
        {
            public Instrument Instrument { get; set; }
        }

        public class RefInstrument : Instrument
        {
            public Dut Dut { get; set; }
        }

        [Test]
        public void XDocSerializerNumberList()
        {
            InstrumentSettings.Current.Clear();
            InstrumentSettings.Current.Add(new ListTestInstr() { Name = "INSTR1" });
            InstrumentSettings.Current.Add(new ListTestInstr() { Name = "INSTR2" });

            var refInstr = new RefInstrument();
            var refDut = new RefDut();
            refDut.Instrument = refInstr;
            refInstr.Dut = refDut;

            InstrumentSettings.Current.Add(refInstr);
            DutSettings.Current.Clear();
            DutSettings.Current.Add(refDut);

            {
                using (var memstr = new MemoryStream())
                {
                    var serializer = new TapSerializer();
                    serializer.Serialize(memstr, InstrumentSettings.Current);
                    var str = Encoding.UTF8.GetString(memstr.ToArray());
                    memstr.Position = 0;
                    var deserializer = new TapSerializer();
                    var instruments = (IList)deserializer.Deserialize(memstr);
                    var refdut2 = instruments.OfType<RefInstrument>().FirstOrDefault().Dut;
                    Assert.AreEqual(refDut, refdut2);
                }

                using (var memstr = new MemoryStream())
                {
                    var serializer = new TapSerializer();
                    serializer.Serialize(memstr, DutSettings.Current);
                    var str = Encoding.UTF8.GetString(memstr.ToArray());
                    memstr.Position = 0;
                    var deserializer = new TapSerializer();
                    var duts = (IList)deserializer.Deserialize(memstr);
                    Assert.AreEqual(refInstr, duts.OfType<RefDut>().FirstOrDefault().Instrument);
                }
            }

            try
            {
                ListStep step1 = new ListStep();
                ListStep step3 = new ListStep();
                step1.Name = "STEP1";
                step3.Name = "STEP3";
                DynamicStepTest dynstep = new DynamicStepTest() { NewData = "Hello" + new string('A', 50) };
                DynamicStepTest dynstep2 = new DynamicStepTest() { NewData = "Hello" + new string('B', 5000) };

                var someNumberProp = TypeData.FromType(typeof(ListStep)).GetMember("SomeNumber");

                step1.Load();
                step3.Load();
                step1.OtherStep = step3;
                step3.OtherStep = step1;
                TestPlan plan = new TestPlan();
                plan.ChildTestSteps.Add(step1);
                plan.ChildTestSteps.Add(step3);
                plan.ChildTestSteps.Add(dynstep);
                plan.ChildTestSteps.Add(dynstep2);
                // external parameters must be added after the steps has been inserted
                // into the test plan.
                plan.ExternalParameters.Add(step1, someNumberProp, "SomeNumber");
                TestPlan plan2;
                using (var memstr = new MemoryStream())
                {
                    var serializer = new TapSerializer();
                    serializer.Serialize(memstr, plan);

                    // Swap the instrument settings.
                    InstrumentSettings.Current.Insert(0, InstrumentSettings.Current[1]);
                    InstrumentSettings.Current.RemoveAt(2);

                    var str = Encoding.UTF8.GetString(memstr.ToArray());
                    memstr.Position = 0;
                    var deserializer = new TapSerializer();
                    plan2 = (TestPlan)deserializer.Deserialize(memstr);
                }
                var step2 = (ListStep)plan2.ChildTestSteps[0];
                var step4 = (ListStep)plan2.ChildTestSteps[1];
                DynamicStepTest dyn2 = (DynamicStepTest)plan2.ChildTestSteps[2];
                DynamicStepTest dyn3 = (DynamicStepTest)plan2.ChildTestSteps[3];
                Assert.AreEqual("HelloHello" + new string('A', 50), dyn2.NewData2);
                Assert.AreEqual("HelloHello" + new string('B', 5000), dyn3.NewData2);
                step2.CheckSame(step1);
                step4.CheckSame(step3);
                Assert.AreNotEqual(step2, step4);
                Assert.AreEqual(step2.OtherStep, step4);
                Assert.AreEqual(step4.OtherStep, step2);

                Assert.IsTrue(step2.WasDeserialized);
                Assert.IsNull(step2.NullThing);
                var ext = plan2.ExternalParameters.Get("SomeNumber");
                Assert.IsNotNull(ext);
                Assert.AreEqual((double)ext.Value, step1.SomeNumber);
                Assert.AreEqual(step2.Name, step1.Name);
            }
            finally
            {
                InstrumentSettings.Current.RemoveIf<IInstrument>(x => x is ListTestInstr || x is RefInstrument);
                DutSettings.Current.RemoveIf<IDut>(x => x is RefDut);
            }
        }

        [Test]
        public void StepIdChangeStep()
        {
            //
            // this test verifies that a step heirarchy can be loaded 
            // without the IDs changes. 
            //

            var delay1 = new DelayStep();
            var delay2 = new DelayStep();
            var seq = new SequenceStep();
            var plan = new TestPlan();
            seq.ChildTestSteps.Add(delay1);
            seq.ChildTestSteps.Add(delay2);
            plan.ChildTestSteps.Add(seq);
            using (var memstr = new MemoryStream())
            {
                plan.Save(memstr);
                memstr.Position = 0;
                var plan2 = TestPlan.Load(memstr, "Untitled");
                var seq2 = plan2.ChildTestSteps[0] as SequenceStep;
                
                Assert.IsTrue(plan2.ChildTestSteps[0].Id == seq.Id);
                Assert.IsTrue(seq2.ChildTestSteps[0].Id == delay1.Id);
                Assert.IsTrue(seq2.ChildTestSteps[1].Id == delay2.Id);
            }
        }


        [Test]
        public void SerializePLanWithIntegerResourceNames()
        {
            InstrumentSettings.Current.Clear();

            InstrumentSettings.Current.Add(new ScpiInstrument { Name = "1" });
            InstrumentSettings.Current.Add(new ScpiInstrument { Name = "0" });

            TestPlan tp = new TestPlan();
            tp.ChildTestSteps.Add(new SCPIRegexStep { Instrument = InstrumentSettings.Current[0] as ScpiInstrument });
            using (var ms = new MemoryStream())
            {
                tp.Save(ms);

                ms.Seek(0, 0);

                tp = TestPlan.Load(ms, tp.Path);
            }

            Assert.AreSame(InstrumentSettings.Current[0], (tp.ChildTestSteps[0] as SCPIRegexStep).Instrument);
        }

        void checkTypeToXml(Type type, string expected = null)
        {
            var strversion = TapSerializer.TypeToXmlString(type);
            new System.Xml.Linq.XElement(strversion);
            if (expected != null)
                Assert.AreEqual(expected, strversion);
        }
        public class µX_y_zµ { }
        public class NestedUnicode﹎ࠦ { }

        [Test]
        public void TypeToXmlNameTest()
        {
            checkTypeToXml(typeof(TestPlan), "TestPlan");
            checkTypeToXml(typeof(List<int>), "ListOfInt32");
            checkTypeToXml(typeof(string[]), "ArrayOfString");
            checkTypeToXml(typeof(ICollection<List<string[]>>));
            checkTypeToXml(typeof(List<>));
            checkTypeToXml(new { x = 5, c = 1 }.GetType());
            checkTypeToXml(new Action(() => { }).GetType());
            checkTypeToXml(typeof(NestedUnicode﹎ࠦ));
            checkTypeToXml(typeof(µX_y_zµ), "_X_y_z_");
        }

        /// <summary>
        /// Between 7.0 and 7.2 we did major changes to serialization.
        /// </summary>
        [Test]
        [Ignore("We are not currently backwards compatible with TAP 7.")]
        public void LoadSweepForTap70()
        {
            var serialized = (TestPlan)new TapSerializer().DeserializeFromFile("TestTestPlans/tap70_sweep_loop.TapPlan");
            var sl = (SweepLoop)serialized.ChildTestSteps[0];
            Assert.AreEqual(3, sl.SweepParameters.Count);
            Assert.AreEqual(typeof(string), sl.SweepParameters[0].Type);
            Assert.AreEqual(typeof(InputButtons), sl.SweepParameters[1].Type);
            Assert.AreEqual(typeof(double), sl.SweepParameters[2].Type);
            Assert.AreEqual("1", (string)sl.SweepParameters[0].Values.GetValue(0));
            Assert.AreEqual(InputButtons.OkCancel, (InputButtons)sl.SweepParameters[1].Values.GetValue(0));
            Assert.AreEqual(true, sl.EnabledRows[0]);
        }

        public class DefaultValueTestStep : TestStep
        {
            [DefaultValue("test")]
            public string Value { get; set; } = "";
            public override void Run()
            {    
                throw new NotImplementedException();
            }
        }

        [TestCase("test", false)]
        [TestCase("SomeOtherString", true)]
        [TestCase("", true)]
        [TestCase(null, true)]
        public void DefaultValueTest(string value, bool serialized)
        {
            var logStep = new DefaultValueTestStep();
            var plan = new TestPlan();
            plan.Steps.Add(logStep);
            logStep.Value = value;

            using (var mem = new MemoryStream())
            {
                plan.Save(mem);
                mem.Seek(0, SeekOrigin.Begin);
                plan = TestPlan.Load(mem, "plan");
            }

            logStep = (DefaultValueTestStep)plan.Steps[0];
            // Expect value to be equal since it is not the same as default value now
            if (serialized)
                Assert.AreEqual(value, logStep.Value);
            // Expect property from test step to be empty since it is the same default value as the one from constructor
            else
                Assert.AreNotEqual(value, logStep.Value);
        }

        // Technically speaking, DefaultValueAttribute is not supported in the sense that properties with default value
        // does not get serialized, except for some special cases.
        // This feature would conflict with External Parameters as it requires there to be an element and also provide strange behaviors if the
        // default value later gets changed for a given test plan.
        [TestCase(null)]
        [TestCase("test")]
        [TestCase("")]
        public void ExternalStringValue(string value)
        {
            var logStep = new DefaultValueTestStep();
            var plan = new TestPlan();
            plan.Steps.Add(logStep);
            plan.ExternalParameters.Add(logStep, TypeData.GetTypeData(logStep).GetMember(nameof(DefaultValueTestStep.Value)));
            logStep.Value = value;
            Assert.IsNotNull(plan.ExternalParameters.Find(logStep, TypeData.GetTypeData(logStep).GetMember(nameof(DefaultValueTestStep.Value))));

            using (var mem = new MemoryStream())
            {
                plan.Save(mem);
                mem.Seek(0, SeekOrigin.Begin);
                plan = TestPlan.Load(mem, "plan");
            }

            logStep = (DefaultValueTestStep)plan.Steps[0];
            Assert.IsNotNull(plan.ExternalParameters.Find(logStep, TypeData.GetTypeData(logStep).GetMember(nameof(DefaultValueTestStep.Value))));
            Assert.AreEqual(value, logStep.Value);
        }
    }

    [TestFixture]
    public class ResourceReference
    {
        public class RefListener : ResultListener
        {
            public IInstrument InstrRef { get; set; }
        }

        [Test]
        public void SerializeResourceRefInResource()
        {
            try
            {
                // Make sure we have two profiles with a SCPIInstrument in each:
                ComponentSettings.SetSettingsProfile("Bench", "Test1");
                InstrumentSettings.Current.Clear();
                InstrumentSettings.Current.Add(new DummyInstrument { Name = "Dum" });
                InstrumentSettings.Current.Add(new ScpiInstrument { Name = "INST1" });
                InstrumentSettings.Current.Save();
                ComponentSettings.SetSettingsProfile("Bench", "Test2");
                InstrumentSettings.Current.Clear();
                InstrumentSettings.Current.Add(new ScpiInstrument { Name = "INST2" });
                InstrumentSettings.Current.Save();

                // Add a RefListener to ResultSettings
                var res = new RefListener();
                ResultSettings.Current.RemoveIf<IResultListener>(l => l is RefListener);
                ResultSettings.Current.Add(res);
                res.InstrRef = InstrumentSettings.Current.GetDefault<ScpiInstrument>();
                ResultSettings.Current.Save();

                Assert.AreEqual("INST2", res.InstrRef.Name);

                // Switch profile
                ComponentSettings.SetSettingsProfile("Bench", "Test1");

                // Reload ResultSettings
                var invRes = ResultSettings.Current.GetDefault<RefListener>();

                // Check that Instument reference now refers to instument from other profile
                Assert.IsNotNull(invRes.InstrRef);
                Assert.AreEqual("INST1", invRes.InstrRef.Name);
            }
            finally
            {
                ResultSettings.Current.Clear();
                ResultSettings.Current.Save();
                InstrumentSettings.Current.Clear();
            }
        }

        class CloneEnforcer : ITapSerializerPlugin
        {
            public double Order => 10;

            public bool Deserialize(XElement node, ITypeData t, Action<object> setter)
            {
                return false;
            }

            public Object TheObject;
            public bool Serialize(XElement node, object obj, ITypeData expectedType)
            {
                if (obj == TheObject)
                {
                    node.SetAttributeValue("Source", "None");
                }
                return false;
            }
        }

        [Test]
        public void SkipSerializePluginResourceTest()
        {
            var newinst = new LogResultListener() { FilePath = new MacroString() { Text = "hello" } };
            ResultSettings.Current.Add(newinst);
            var serializer = new TapSerializer();

            var clone1 = (LogResultListener)serializer.Clone(newinst); 
            // not actually a clone.
            Assert.AreSame(newinst, clone1);
            Assert.AreEqual(newinst.FilePath.Text, clone1.FilePath.Text);

            serializer.AddSerializers(new[] { new CloneEnforcer() { TheObject = newinst } });

            // a memberwise clone.
            var clone2 = (LogResultListener)serializer.Clone(newinst);
            Assert.AreNotSame(newinst, clone2);
            Assert.AreEqual(newinst.FilePath.Text, clone1.FilePath.Text);
        }

    }

    [TestFixture]
    public class StringConvertProviderTest
    {
        void ObjectAssertEqual(object expected, object actual)
        {
            Assert.AreEqual(expected, actual);
        }

        void ObjectAssertEqual(IEnabled expected, IEnabled actual)
        {
            Assert.AreEqual(expected.IsEnabled, actual.IsEnabled);
            dynamic _expected = expected, _actual = actual;
            ObjectAssertEqual(_expected.Value, _actual.Value);

        }

        void ObjectAssertEqual(IEnumerable expected, IEnumerable actual)
        {
            var l1 = expected.Cast<dynamic>().ToArray();
            var l2 = actual.Cast<dynamic>().ToArray();
            Assert.AreEqual(l1.Length, l2.Length);
            for (int i = 0; i < l1.Length; i++)
            {
                ObjectAssertEqual(l1[i], l2[i]);
            }
        }

        void DynObjectAssertEqual(dynamic expected, dynamic actual)
        {
            ObjectAssertEqual(expected, actual);
        }

        void testStringConvert(object value)
        {
            var strfmt = StringConvertProvider.GetString(value);
            var reparse = StringConvertProvider.FromString(strfmt, TypeData.FromType(value.GetType()), null);
            DynObjectAssertEqual(value, reparse);
        }

        [Test]
        public void StringConvertTest()
        {
            // IConvertible
            testStringConvert(5);
            testStringConvert("ASD");
            testStringConvert("");
            testStringConvert("µX_y_zµ");
            testStringConvert(new DateTime(2017, 3, 3, 3, 3, 3, 0)); // note: milliseconds are not parsed into the datetime.

            // Enabled
            testStringConvert(new Enabled<int>() { Value = 10, IsEnabled = false });
            testStringConvert(new Enabled<int>() { Value = 20, IsEnabled = true });

            // Enum
            testStringConvert(Verdict.Fail);
            testStringConvert(Verdict.Pass);
            testStringConvert(EngineSettings.AbortTestPlanType.Step_Error | EngineSettings.AbortTestPlanType.Step_Fail);

            // Resources
            var instr1 = new TestPortInstrument() { Name = "MyInstr" };

            var instr2 = new TestPortInstrument() { Name = "MyInstr 2" };
            var dut1 = new FourPortDut() { Name = "MyDut1" };

            InstrumentSettings.Current.Add(instr1);
            InstrumentSettings.Current.Add(instr2);
            DutSettings.Current.Add(dut1);
            try
            {
                testStringConvert(instr1);
                testStringConvert(instr2);
                testStringConvert(dut1);
                testStringConvert(new List<IResource> { instr1, instr2, dut1 });
                testStringConvert(new List<IResource> { });
            }
            finally
            {
                InstrumentSettings.Current.Remove(instr1);
                InstrumentSettings.Current.Remove(instr2);
                DutSettings.Current.Remove(dut1);
            }

            // Collections.
            testStringConvert(new Verdict[] { Verdict.Pass, Verdict.Fail, Verdict.Aborted });
            testStringConvert(new LogSeverity[] { LogSeverity.Debug, LogSeverity.Error, LogSeverity.Warning });
            testStringConvert(new EngineSettings.AbortTestPlanType[] { EngineSettings.AbortTestPlanType.Step_Error, EngineSettings.AbortTestPlanType.Step_Error | EngineSettings.AbortTestPlanType.Step_Fail });
            testStringConvert(new double[] { });
            testStringConvert(new double[] { 1, 2, 3, 4, 7, 8, 9, 0 });
            testStringConvert(new List<int> { 1, 2, 3, 4, 7, 8, 9, 0 });
            testStringConvert(new List<string> { "A A\"\" A A,", "B \" B B,", "C,D", "" });
            testStringConvert(new List<Verdict> { Verdict.Pass, Verdict.Fail,Verdict.Pass,Verdict.Aborted });
            testStringConvert(new List<Verdict> { });
            testStringConvert(new List<EngineSettings.AbortTestPlanType> { EngineSettings.AbortTestPlanType.Step_Error | EngineSettings.AbortTestPlanType.Step_Fail, EngineSettings.AbortTestPlanType.Step_Error });

            var reparse = (Verdict)StringConvertProvider.FromString("pass", TypeData.FromType(typeof(Verdict)), null);
            Assert.AreEqual(Verdict.Pass, reparse);
        }

        class SubObjTest
        {
            public double X { get; set; } 
            public double Y { get; set; } 
        } 

    
        [Test]
        public void FailingStringConvertTest()
        {
            // this should _not_ work
            var subobj = new SubObjTest();
            List<SubObjTest> subObjects = new List<SubObjTest>() {subobj};
            bool passed = StringConvertProvider.TryGetString(subObjects, out string result);
            Assert.IsFalse(passed);
        }

        public class AnyObjectClass : TestStep
        {
            public object Item { get; set; }
            public override void Run()
            {
                
            }
        }

        public struct Vec3d
        {
            public double X, Y, Z;
        }


        // To fully support IPaddresses, we also need to be able to serialize/deserialize them.
        // This plugin class takes care of that.
        public class Vec3dSerializer : ITapSerializerPlugin
        {
            public double Order => 5;

            public bool Deserialize(XElement node, ITypeData t, Action<object> setter)
            {
                if(t == TypeData.FromType(typeof(Vec3d)) == false) return false;
                var values = node.Value.Trim().TrimStart('(').TrimEnd(')')
                    .Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(double.Parse).ToArray();
                setter(new Vec3d{X = values[0], Y = values[1], Z = values[2]});
                return true;
            }

            public bool Serialize(XElement node, object obj, ITypeData expectedType)
            {
                if(obj is Vec3d v)
                {
                    node.Value = $"({v.X} {v.Y} {v.Z})";
                    return true;
                }
                return false;
            }
        }

        [Test]
        public void AnyObjectSerializeTest()
        {
            var obj = new AnyObjectClass() {Item = new Vec3d(){Y = 10}};
            var plan = new TestPlan();
            plan.Steps.Add(obj);
            TypeData.GetTypeData(obj).GetMember("Item").Parameterize(plan, obj, "Item");
            
            var str = new TapSerializer().SerializeToString(plan);
            var obj2 = (TestPlan)new TapSerializer().DeserializeFromString(str);
            var externalParameter = obj2.ExternalParameters.Get("Item");
            Assert.IsNotNull(externalParameter);
            Assert.AreEqual(10.0, ((Vec3d) externalParameter.Value).Y);
        }

        [Test]
        public void DeserializeLostInputProperty()
        {
            var plan1 = new TestPlan();
            var repeat = new RepeatStep();
            var log = new LogStep();
            plan1.ChildTestSteps.Add(repeat);
            repeat.ChildTestSteps.Add(log);
            var member = TypeData.GetTypeData(log).GetMember(nameof(LogStep.LogMessage));
            var outputMember = TypeData.GetTypeData(repeat).GetMember(nameof(RepeatStep.IterationInfo));
            InputOutputRelation.Assign(log, member, repeat, outputMember);

            var steps = new ITestStep[] {repeat, log};
            var xml = new TapSerializer().SerializeToString(steps);

            var ser2 = new TapSerializer();
            
            var deserialized = (ITestStep[]) ser2.DeserializeFromString(xml);
            var plan = new TestPlan();
            plan.Steps.AddRange(deserialized);
            
            var repeat2 = deserialized[0];
            var delay2 = deserialized[1];
            Assert.IsTrue(InputOutputRelation.IsOutput(repeat2, outputMember));
            Assert.IsTrue(InputOutputRelation.IsInput(delay2, member));
        }
    }

    

    [TestFixture]
    public class SecureStringSerializerTest
    {
        public class SomeInstrument
        {
            public string UserName { get; set; } = "XYZ";
            public System.Security.SecureString Password { get; set; } = new System.Security.SecureString();
        }

        [Test]
        public void SerializationTest()
        {
            SomeInstrument inst = new SomeInstrument();
            char[] chars = new char[] { 's', 'e', 'c', 'r', 'e', 't' };
            foreach(char c in chars)
                inst.Password.AppendChar(c);
            
            string xml = new TapSerializer().SerializeToString(inst);
            var inst2 = (SomeInstrument)new TapSerializer().DeserializeFromString(xml, TypeData.GetTypeData(inst));
            Assert.AreEqual(inst.Password.ToString(), inst2.Password.ToString());
        }
    }

    public class XmlTextAttributeTest
    {
        public enum Mode
        {
            A,
            B,
            C
        }

        public class SimpleXmlTestAttribute
        {
            [XmlText(Type = typeof(Mode))] public Mode Value { get; set; }
        }

        [Test]
        public void SimpleXmlTextAttributeTest()
        {
            var myGroup1 = new SimpleXmlTestAttribute {Value = Mode.C};
            var str = new TapSerializer().SerializeToString(myGroup1);
            var xml = XDocument.Load(new MemoryStream(Encoding.UTF8.GetBytes(str)));


            var elem = xml.Element("SimpleXmlTestAttribute");
            Assert.IsNotNull(elem);
            Assert.AreEqual(elem.Value, nameof(Mode.C));
            // Should not have child element Value since it is serialized as XmlText
            Assert.IsNull(elem.Element("Value"));
        }

        public class SerializeConnectionTestStep : TestStep
        {
            [AvailableValues(nameof(availableInstruments))]
            public Instrument myInstrument_withAvailableValues { get; set; }

            [AvailableValues(nameof(availableConnections))]
            public Connection myConnection_withAvailableValues { get; set; }

            public List<Connection> availableConnections
            {
                get { return ConnectionSettings.Current.Select(x => x as Connection).Where(c => c != null).ToList(); }
            }

            public List<Instrument> availableInstruments
            {
                get { return InstrumentSettings.Current.Select(x => x as Instrument).Where(c => c != null).ToList(); }
            }

            public override void Run() => throw new NotImplementedException();
        }

        [Test]
        public void SerializeComponentSettingsTest()
        {

            try
            {

                ConnectionSettings.Current.Add(new RfConnection());
                InstrumentSettings.Current.Add(new ScpiInstrument { VisaAddress = "1234" });
                
                var testPlan = new TestPlan();
                var step = new SerializeConnectionTestStep();
                testPlan.ChildTestSteps.Add(step);

                var conn = step.availableConnections.First();
                var instr = step.availableInstruments.First();

                Assert.NotNull(conn);
                Assert.NotNull(instr);

                step.myConnection_withAvailableValues = conn;
                step.myInstrument_withAvailableValues = instr;

                Assert.AreSame(step.myConnection_withAvailableValues, conn);
                Assert.AreSame(step.myInstrument_withAvailableValues, instr);

                byte[] planData;

                using (MemoryStream ms = new MemoryStream(20000))
                {
                    testPlan.Save(ms);
                    planData = ms.ToArray();
                }

                using (MemoryStream ms = new MemoryStream(planData))
                    testPlan = testPlan.Reload(ms);

                var newStep = testPlan.ChildTestSteps.First() as SerializeConnectionTestStep;

                Assert.NotNull(newStep);
                Assert.NotNull(newStep.myConnection_withAvailableValues);
                Assert.NotNull(newStep.myInstrument_withAvailableValues);
                Assert.AreSame(newStep.myInstrument_withAvailableValues, instr);
                Assert.AreSame(newStep.myConnection_withAvailableValues, conn);
            }
            finally
            {
                ConnectionSettings.Current.Clear();
                InstrumentSettings.Current.Clear();
            }

        }

        [Test]
        public void NullInstrumentTest()
        {
             object outValue = new ObjectCloner(null).Clone(true, null, TypeData.FromType(typeof(ScpiInstrument)));
             Assert.IsNull(outValue);
        }
        
        public class InPlaceProperty : ValidatingObject
        {
            int x;
            public int X
            {
                get => x;
                set
                {
                    x = value;
                    OnPropertyChanged(nameof(X));
                } 
            }

            public InPlaceProperty()
            {
                
            }
        }
        public class InPlacePropertyOwner
        {
            InPlaceProperty prop = new InPlaceProperty();

            [DeserializeInPlace]
            public InPlaceProperty Prop
            {
                get => prop;
                set => throw new Exception("This should not be set.");
            }

            public int XChanged = 0;
            public InPlacePropertyOwner()
            {
                prop.PropertyChanged += PropOnPropertyChanged;
            }

            void PropOnPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                XChanged += 1;
            }
        }
        
        [Test]
        public void TestInPlaceProperty()
        {
            var a = new InPlacePropertyOwner();
            a.Prop.X += 1;
            a.Prop.X += 2;
            var xml = new TapSerializer().SerializeToString(a);
            var b = (InPlacePropertyOwner) new TapSerializer().DeserializeFromString(xml);
            Assert.AreEqual(1, b.XChanged);
            Assert.AreEqual(3, b.Prop.X);
        }

        [Test]
        public void TestBase64EncodeDecode()
        {
            var testMessage = "(SCPI:TRIG1:SEQ:AIQM:DEL:STAT) ";
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(testMessage));
            
            var plan = new TestPlan();
            plan.ChildTestSteps.Add(new DialogStep() { Message = "(SCPI:TRIG1:SEQ:AIQM:DEL:STAT) "});
            
            var ser = new TapSerializer();
            var planXml = ser.SerializeToString(plan);
            
            StringAssert.Contains($"<Base64>{base64}</Base64>", planXml);

            var newPlan = ser.DeserializeFromString(planXml) as TestPlan;

            var dialog = newPlan.ChildTestSteps.First() as DialogStep;
            Assert.AreEqual(testMessage, dialog.Message);
        }
        
        [Test]
        public void TestBase64DecodeWithNamespace()
        {
            var testMessage = "(SCPI:TRIG1:SEQ:AIQM:DEL:STAT) ";
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(testMessage));
            var ns = "http://opentap.io/schemas/package";
            var xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<TestPlan type=""OpenTap.TestPlan"" Locked=""false"" xmlns=""{ns}"">
  <Steps>
    <TestStep type=""OpenTap.Plugins.BasicSteps.DialogStep"" Version=""9.4.0-Development"" Id=""d656ee5b-e764-4c6f-be9f-e4f28bcdee13"">
      <Message>
        <Base64>{base64}</Base64>
      </Message>
      <Title>Title</Title>
      <Buttons>OkCancel</Buttons>
      <PositiveAnswer>NotSet</PositiveAnswer>
      <NegativeAnswer>NotSet</NegativeAnswer>
      <UseTimeout>false</UseTimeout>
      <Timeout>5</Timeout>
      <DefaultAnswer>NotSet</DefaultAnswer>
      <Enabled>true</Enabled>
      <Name>Dialog</Name>
      <ChildTestSteps />
      <BreakConditions>Inherit</BreakConditions>
    </TestStep>
  </Steps>
  <BreakConditions>Inherit</BreakConditions>
  <OpenTap.Description />
  <Package.Dependencies />
</TestPlan>
";
            var ser = new TapSerializer();
            var plan = ser.DeserializeFromString(xml) as TestPlan;
            var dialog = plan.ChildTestSteps.First() as DialogStep;
            Assert.AreEqual(testMessage, dialog.Message);
        }

        [Test]
        public void TestSerializeProcessStepWithNullDefaultValue()
        {
            // When DefaulValue is used and the property value is null an exception was thrown.
            var plan = new TestPlan();
            var step = new ProcessStep {LogHeader = null};
            plan.ChildTestSteps.Add(step);
            plan.SerializeToString(true);
        }

        /// <summary> This class just inhertis from test plan </summary>
        public class TestPlanTest2 : TestPlan
        {
            
        }

        /// <summary>
        /// A problem existed where if inheriting from TestPlan,
        /// Name was not set to the right value after deserializing from a stream.
        /// </summary>
        [Test]
        public void TestSerializeInheritTestPlan()
        {
            var tp = new TestPlanTest2();
            // this once threw an exception because of an error in ChildTestSteps.Add.
            tp.ChildTestSteps.Add(new DelayStep());
            var str = new MemoryStream();
            tp.Save(str);
            str.Seek(0, SeekOrigin.Begin);
            tp = (TestPlanTest2)TestPlan.Load(str, "testplantest2.TapPlan");
            // before the fix, this would be set "Untitled"
            Assert.AreEqual(tp.Name, "testplantest2");
        }
        
        [Test]
        public void TestSerializerErrors()
        {
            var x = new Enabled<Enabled<Enabled<int>>>();
            x.Value = new Enabled<Enabled<int>>();
            x.Value.Value = new Enabled<int>();
            x.Value.Value.Value = 4;
            var ser = new TapSerializer();
            var str = ser.SerializeToString(x);
            Assert.IsFalse(ser.XmlErrors.Any());
            var str2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
                <EnabledOfEnabledOfEnabledOfInt32 type=""OpenTap.Enabled`1[[OpenTap.Enabled`1[[OpenTap.Enabled`1[[System.Int32, System.Private.CoreLib, Version=6.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]], OpenTap, Version=9.4.0.0, Culture=neutral, PublicKeyToken=null]], OpenTap, Version=9.4.0.0, Culture=neutral, PublicKeyToken=null]]"">
                <Value>
                <Value>
                <Value>abc</Value>
                <IsEnabled>false</IsEnabled>
                </Value>
                <IsEnabled>false</IsEnabled>
                </Value>
                <IsEnabled>false</IsEnabled>
                </EnabledOfEnabledOfEnabledOfInt32>";
            var r= ser.DeserializeFromString(str2, TypeData.GetTypeData(x));
            Assert.AreEqual(1, ser.XmlErrors.Count());
            var err = ser.XmlErrors.First();
            // error was that asd could not be parsed as an int.
            Assert.AreEqual("abc", err.Element.Value);
            // error occured on line number 5
            Assert.AreEqual(5, (err.Element as IXmlLineInfo).LineNumber);
        }
    }
}
