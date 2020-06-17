using System.Globalization;
using System.Linq;
using System.Reflection;

namespace OpenTap.Plugins.BasicSteps
{
    [AllowAnyChild]
    [Display("Sweep Parameter", "Table based loop that sweeps the value of its parameters based on a set of values.", "Flow Control")]
    public class SweepParameterStep : SweepParameterStepBase
    {
        SweepRowCollection sweepValues = new SweepRowCollection();
        [DeserializeOrder(1)] // this should be deserialized as the last thing.
        [Display("Sweep Values", "A table of values to be swept for the selected parameters.", "Sweep")]
        [HideOnMultiSelect] // todo: In the future support multi-selecting this.
        [Unsweepable]
        public SweepRowCollection SweepValues 
        { 
            get => sweepValues;
            set
            {
                sweepValues = value;
                sweepValues.Loop = this;
            }
        }

        public SweepParameterStep()
        {
            SweepValues.Loop = this;
            SweepValues.Add(new SweepRow());
            Name = "Sweep {ParametersDisplay}";
        }

        int iteration;
        
        [Output]
        [Display("Iteration", "Shows the iteration of the sweep that is currently running or about to run.", "Sweep", Order: 3)]
        public string IterationInfo => $"{iteration} of {SweepValues.Count(x => x.Enabled)}";

        public override void PrePlanRun()
        {
            base.PrePlanRun();
            iteration = 0;
        }

        public override void Run()
        {
            base.Run();
            iteration = 0;
            var sets = SelectedMembers.ToArray();
            var originalValues = sets.Select(set => set.GetValue(this)).ToArray();

            var rowType = SweepValues.Select(TypeData.GetTypeData).FirstOrDefault();
            foreach (var Value in SweepValues)
            {
                if (Value.Enabled == false) continue;
                var AdditionalParams = new ResultParameters();

                
                foreach (var set in sets)
                {
                    var mem = rowType.GetMember(set.Name);
                    var val = StringConvertProvider.GetString(mem.GetValue(Value), CultureInfo.InvariantCulture);
                    var disp = mem.GetDisplayAttribute();
                    AdditionalParams.Add(new ResultParameter(disp.Group.FirstOrDefault() ?? "", disp.Name, val));

                    try
                    {
                        var value = StringConvertProvider.FromString(val, set.TypeDescriptor, this, CultureInfo.InvariantCulture);
                        set.SetValue(this, value);
                    }
                    catch (TargetInvocationException ex)
                    {
                        Log.Error("Unable to set '{0}' to value '{2}': {1}", set.GetDisplayAttribute().Name, ex?.InnerException?.Message, val);
                        Log.Debug(ex.InnerException);
                    }
                }

                iteration += 1;
                // Notify that values might have changes
                OnPropertyChanged("");
                
                 Log.Info("Running child steps with {0}", Value.GetIterationString());

                var runs = RunChildSteps(AdditionalParams, BreakLoopRequested).ToList();
                if (BreakLoopRequested.IsCancellationRequested) break;
                runs.ForEach(r => r.WaitForCompletion());
            }
            for (int i = 0; i < sets.Length; i++)
                sets[i].SetValue(this, originalValues[i]);
        } 
    }
}
