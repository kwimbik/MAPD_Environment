using diskretni_mapd_simulace.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using static System.Math;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html;
using iTextSharp.text.html.simpleparser;
using System.Windows.Forms.VisualStyles;
using System.Windows;
using static iTextSharp.text.pdf.events.IndexEvents;
using iTextSharp.text.pdf.parser.clipper;

namespace diskretni_mapd_simulace.Plan_Tools
{
    public class PlanValidator
    {
        Simulace_Visual sv;
        Plan plan;
        int msToWaitForScreenshot = 500;
        int planIndex = 0;
        private string filepath = "sc";
        List<(int, int)> time_windows = new List<(int, int)>();
        byte[][] colors = new byte[6][] {
                 new byte[]{ 255, 0, 0 },
                 new byte[] { 255, 165, 0 },
                 new byte[]{ 0, 0, 255 },
                 new byte[]{ 255, 0, 165 },
                 new byte[]{ 0, 255, 165 },
                 new byte[] { 165, 0, 255 }};

        public PlanValidator(Simulace_Visual sv, Plan plan)
        {
            this.sv = sv;
            this.plan = plan; //steps in plan are given sorted by time
        }

        public object Dispatcher { get; private set; }

        public void Validate()
        {
            time_windows.Clear();
            BackgroundWorker bw = new BackgroundWorker();

            bw.WorkerReportsProgress = true;
            int validationSteps = 0;


            bw.DoWork += new DoWorkEventHandler(
            delegate (object o, DoWorkEventArgs args)
            {
                sv.createValidationPlanMap();

                //assign colors to the agents
                int Index = 0;
                if (plan.agents.Count > colors.Length)
                {
                    foreach (var agent in plan.agents)
                    {
                        agent.color = colors[0];
                    }
                }
                else
                {
                    foreach (var agent in plan.agents)
                    {
                        agent.color = colors[Index++];
                    }
                }

                //dict: AgentID -> List of location ids
                Dictionary<string, List<int>> agentPaths = new Dictionary<string, List<int>>();
                Dictionary<string, int> lastAgentPosition = new Dictionary<string, int>();
                int startTime = 0;
                int endTime = 0;


                while (plan.steps.Count > 0)
                {

                    agentPaths = extractNoncolidingPaths(lastAgentPosition);

                    foreach (KeyValuePair<string, List<int>> entry in agentPaths)
                    {
                        lastAgentPosition[entry.Key] = entry.Value[entry.Value.Count - 1];
                    }

                    foreach (var agent in plan.agents)
                    {
                        if (agentPaths.ContainsKey(agent.id))
                        {
                            sv.colorPath(agentPaths[agent.id], agent.color);
                        }
                    }

                    Thread.Sleep(msToWaitForScreenshot);
                    sv.SaveAsPng(planIndex++);
                    sv.resetPlanGrid(agentPaths);
                    validationSteps++;
                }

                sv.clearValidationGrid(); //calling last to clear up
                expoortPDF(filepath, validationSteps, time_windows);
            });

            bw.RunWorkerAsync();
        }


        //returns list of locations for each agent for one non colliding iteration
        private Dictionary<string , List<int>> extractNoncolidingPaths(Dictionary<string, int> lastPositions)
        {
            List<int> usedLocationIds = new List<int>();

            Dictionary<string, List<int>> agentPaths = new Dictionary<string , List<int>>();
            int time = plan.steps[0].time;
            int index = 0;
            int startTime = plan.steps[0].time;

            foreach (KeyValuePair<string, int> entry in lastPositions)
            {
                agentPaths[entry.Key] = new List<int> { entry.Value };
            }

            List<PlanStep> stepBuffer = new List<PlanStep>();
            bool end = false;
            int endIndex = int.MaxValue;
            while (index < plan.steps.Count && end == false)
            {
                PlanStep ps = plan.steps[index++];
                if (ps.type != (int)PlanStep.types.movement) continue;
                if (usedLocationIds.Contains(ps.locationId))
                {
                    agentPaths[ps.agentId].Add(ps.locationId);
                    endIndex = time;
                }
                else usedLocationIds.Add(ps.locationId);
                
                if (agentPaths.ContainsKey(ps.agentId))
                {
                    agentPaths[ps.agentId].Add(ps.locationId);
                }
                else
                {
                    agentPaths[ps.agentId] = new List<int> { ps.locationId } ;
                }
                time = ps.time;
                if (time > endIndex) end = true;
                
            }
            int endTime = plan.steps[index-1].time;
            time_windows.Add((startTime, endTime));

            //Remove all steps upon time of collision
            if (index >= plan.steps.Count) plan.steps.Clear();
            else plan.steps.RemoveRange(0, index);


            return agentPaths;
        }


        protected void expoortPDF(string fp, int validationSteps, List<(int, int)> time_windows)
        {
            string filePath = "sc";
            Document doc = new Document(PageSize.A4, 10f, 10f, 100f, 0f);
            string pdfFilePath = filePath;
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream($"plan_validation/{plan.mapName}_validation.pdf", FileMode.Create));
            doc.Open();
            Paragraph paragraph = new Paragraph("Plan validation");
            doc.Add(paragraph);
            for (int i = 0; i < validationSteps; i++)
            {
                try
                {
                    string imageURL = filePath + $"/Plan{i}.jpg";
                    iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageURL);
                    //Resize image depend upon your need
                    jpg.ScaleToFit(580f, 502f);
                    //Give space before image
                    // some space after the image
                    jpg.Alignment = Element.ALIGN_LEFT;
                    doc.Add(jpg);

                    Paragraph c1 = new Paragraph($"Time frame:{time_windows[i].Item1}-{time_windows[i].Item2}");
                    c1.Alignment = Element.ALIGN_CENTER;
                    doc.Add(c1);
                    doc.NewPage();
                }
                catch (Exception ex)
                {
                    break;
                }
            }
            doc.Close();
        }
    }
}
