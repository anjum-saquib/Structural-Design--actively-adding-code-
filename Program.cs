using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Services;
using System.Text;
using System.Threading.Tasks;

namespace concrete_beam_design
{
    internal class ConcreteBeamDesign
    {
        
        static void Main()
        {
            // Grades of concrete and their characteristic strengths (in MPa)
            string[] concreteGrades = { "C20/25", "C25/30", "C28/35", "C32/40", "C35/45", "C40/50", "C50/60" };
            double[] fck = { 20, 25, 28, 32, 35, 40, 50 };

            // Steel reinforcement grades and their yield strengths (in MPa)
            string[] steelGrades = { "S240", "S400", "S500", "S600" };
            double[] fy = { 240, 400, 500, 600 };

            // Concrete grade selection
            Console.WriteLine("Select the grade of the concrete:");
            int concreteChoice = GetChoice(concreteGrades);
            string selectedConcrete = concreteGrades[concreteChoice - 1];
            double concreteStrength = fck[concreteChoice - 1];
            Console.WriteLine($"You selected concrete grade: {selectedConcrete} (Characteristic Strength: {concreteStrength} MPa)");

            // Steel reinforcement grade selection
            Console.WriteLine("\nSelect the grade of the main steel reinforcement:");
            int steelChoice = GetChoice(steelGrades);
            string selectedSteel = steelGrades[steelChoice - 1];
            double steelStrength = fy[steelChoice - 1];
            Console.WriteLine($"You selected steel reinforcement grade: {selectedSteel} (Yield Strength: {steelStrength} MPa) as main reinfrocement");
            Console.WriteLine("\nSelect the grade of the secondary steel reinforcement:");
            int secondarySteelChoice = GetChoice(steelGrades);
            string selectedSecondarySteel = steelGrades[secondarySteelChoice - 1];
            double secondarySteelStrength = fy[secondarySteelChoice - 1];
            Console.Write($"You selected steel reinforcement grade: {selectedSecondarySteel} (Yield Strength: {secondarySteelStrength} MPa) as secondary reinfrocement");


            // Fix the dimensions of the beam
            Console.WriteLine("\nEnter the dimensions of the beam in mm:");
            double d = GetDoubleInput("Depth of the beam (d) in mm: ");
            double b = GetDoubleInput("Width of the beam (b) in mm: ");
            // Prompt user to enter the clear cover and diameter of main reinforcement bars
            Console.WriteLine("\nEnter the clear cover and diameter of reinforcement bars:");
            double c = GetDoubleInput("Clear cover to reinforcement (c) in mm: ");
            double phi = GetDoubleInput("Diameter of main reinforcement bars (φ) in mm: ");
            double phi_bar = GetDoubleInput("Diameter of secondary reinforcement bars (φ_bar) in mm: ");

            // Calculate the effective depth of the beam
            double d_eff = Calculate(d, c, phi, phi_bar);
            Console.WriteLine($"\nEffective Depth (d_eff) of the beam: {d_eff} mm");

            //Loads
            Console.WriteLine("\nEnter the loads acting on the beam:");
            double deadLoad = GetDoubleInput("Dead Load (DL) in kN/m: ");
            double liveLoad = GetDoubleInput("Live Load (LL) in kN/m: ");
            double selfWeight = CalculateSelfWeight(d, b);
            Console.WriteLine($"Self Weight of the beam: {selfWeight} kN/m");
            double totalLoad = DesignLoad(deadLoad, liveLoad, selfWeight);
            Console.WriteLine($"Total Load on the beam: {totalLoad} kN/m");

            // Calculate design bending moment and shear force
            double spanLength = GetDoubleInput("\nEnter the effective span length of the beam in m: ");
            double designMoment = Moment(totalLoad, spanLength);
            double designShearForce = ShearForce(totalLoad, spanLength);
            Console.WriteLine($"\nDesign Bending Moment (M_d) for the beam: {designMoment} kNm");
            Console.WriteLine($"Design Shear Force (V_d) for the beam: {designShearForce} kN");

            // Calculate the ultimate moment capacity
            Console.WriteLine("\nCalculating the ultimate moment capacity of the beam:");
            double UltimatemomentCapacity = momentCapacity(concreteStrength, b, d_eff);
            Console.WriteLine($"Ultimate Moment Capacity (M_u) of the beam: {UltimatemomentCapacity} kNm");
            // Check if the beam is adequate

            // Calculate the design moment and area of steel required
            Console.WriteLine("\nCalculating the main reinforcement:");
            double k = CalculateK(designMoment, concreteStrength, b, d_eff);
            Console.WriteLine($" k : {k}");
            double leverArm = CalculateLeverArm(d_eff, k);
            Console.WriteLine($"Lever Arm (z) of the beam: {leverArm} mm");
            double Ast = CalculateofAst(designMoment, steelStrength, leverArm);
            Console.WriteLine($"Area of Steel Reinforcement Required (A_st) for the beam: {Ast} mm²");

            // Calculate the number of main reinforcement bars required
            double numberOfBars = CalculateNumberOfBars(Ast, phi);
            Console.WriteLine($"Number of Main Reinforcement Bars Required: {numberOfBars}");

            // Calculate the area of steel provided
            double Ast_provided = CalulateAstProvided(numberOfBars, phi);
            Console.WriteLine($"Area of Steel Reinforcement Provided (A_st_provided): {Ast_provided} mm²");

            // Check if the provided reinforcement is adequate
            if (Ast_provided >= Ast)
            {
                Console.WriteLine("The provided main reinforcement is adequate.");
            }
            else
            {
                Console.WriteLine("The provided main reinforcement is NOT adequate. Increase the number of bars or the diameter.");
            }
            // Display the reinforcement details
            double tensileSpacing = GetDoubleInput("Enter the spacing of main bars from BS Table 3.13 in mm:  "); //Table 3.13
            Console.WriteLine($" Provide main reinfrocement of {phi} mm dia bars @ {tensileSpacing} mm c/c.  ");

            // check for shear reinforcement
            Console.WriteLine("\nCalculating the shear reinforcement :");
            Console.WriteLine("Interpolate the adjustment factor for shear reinforcement from BS:");
            double adjustmentFactor = GetDoubleInput("Adjustment factor = ");
            double tau = UltimateShearStress(designShearForce, b, d_eff);
            Console.WriteLine($"Ultimate Shear Stress (τ_u) for the beam: {tau} MPa");
            double percentAst = percentageShear(Ast_provided, b, d_eff);
            Console.WriteLine($"Percentage of Shear Reinforcement Provided: {percentAst} %");
            double tau_c = DesignConcreteShearStress(concreteStrength, adjustmentFactor);
            Console.WriteLine($"Design Concrete Shear Stress (τ_c) for the beam: {tau_c} MPa");

            // Check if the shear reinforcement is adequate
            if (tau <= tau_c)
            {
                Console.WriteLine("The shear reinforcement is adequate.");
            }
            else if (tau < (tau_c + 0.4))
            {
                Console.WriteLine($"τ = {tau:F2} MPa is between τ_c = {tau_c:F2} MPa and τ_c + 0.4 = {(tau_c + 0.4):F2} MPa");
                Console.WriteLine("Nominal links are required.");
                double shearCheck1 = shearcheck1(tau, tau_c);
                Console.WriteLine($"Asv/Sv value: {shearCheck1} MPa");
                double shearSpacing = GetDoubleInput("Enter the spacing of links from BS Table 3.13 in mm:  "); //Table 3.13
                Console.WriteLine($" Provide shear reinfrocement of {phi_bar} mm dia bars @ {shearSpacing} mm c/c.  ");
            }
            else
            {
                // tau is greater than tau_c + 0.4
                Console.WriteLine($"τ = {tau:F2} MPa is greater than τ_c + 0.4 = {(tau_c + 0.4):F2} MPa");
                Console.WriteLine("Shear reinforcement is required.");
                double shearCheck2 = shearcheck2(b, secondarySteelStrength, tau, tau_c);
                Console.WriteLine($"Asv/Sv value: {shearCheck2} MPa");
                double Ast_provided_shear = GetDoubleInput("Enter the area of shear reinforcement provided (A_sv) in mm²: ");
                double percentageShearReinforcement = percentageShear(Ast_provided_shear, b, d_eff);
                Console.WriteLine($"Percentage of Shear Reinforcement Provided: {percentageShearReinforcement} %");
                if (percentageShearReinforcement >= 0.4)
                {
                    Console.WriteLine("The provided shear reinforcement is adequate.");
                }
                else
                {
                    Console.WriteLine("The provided shear reinforcement is NOT adequate. Increase the area of shear reinforcement or use a higher grade of steel.");
                }

                // deflection check
                Console.WriteLine("\nDeflection Check:");
                double deflection = spantodepth(spanLength, d_eff);
                Console.WriteLine($"Span to Effective Depth Ratio (l/d) = {deflection}: ");
                double Mbybd = Mbd(designMoment, b, d_eff);
                Console.WriteLine($"Mbd = {Mbybd}");
                double f_s_value = f_s(steelStrength, Ast, Ast_provided);
                Console.WriteLine($"Stress in the main steel reinforcement (f_s) = {f_s_value} MPa");
                Console.WriteLine("Enter the value of modification factor from Table 3.10, BS 8110");
                double modificationFactor = GetDoubleInput("Modification Factor (m) = ");
                Console.WriteLine($"Modification Factor (m) = {modificationFactor}");
                double permissibledeflectiion = PermissibleDeflection(modificationFactor);
                Console.WriteLine($"Permissible Deflection = {permissibledeflectiion} mm");

                if (deflection > permissibledeflectiion)
                    {
                    Console.WriteLine("The beam does not satisfy the deflection criteria. Consider increasing the depth or using a higher grade of concrete.");
                }
                else
                {
                    Console.WriteLine("The beam satisfies the deflection criteria.");
                }

            }
        }


        // Helper method to display options and get valid input
        static int GetChoice(string[] options)
        {
            // Display the options
            for (int i = 0; i < options.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {options[i]}");
            }

            // Validate the user's input
            int choice;
            do
            {
                Console.Write($"Enter your choice (1-{options.Length}): ");
            } while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > options.Length);

            return choice;
        }
        // Helper method to get a positive double input from the user
        static double GetDoubleInput(string prompt)
        {
            double value;
            do
            {
                Console.Write(prompt);
            } while (!double.TryParse(Console.ReadLine(), out value) || value <= 0);
            return value;
        }

        //Calculate the effective cover
        static double Calculate(double d, double c, double phi, double phi_bar)
        {
            // Effective cover is the distance from the outer surface of the concrete to the center of the main reinforcement
            return d - c - phi - (phi_bar / 2);
        }
        static double CalculateSelfWeight(double d, double b)
        {
            // Assuming concrete density is 25 kN/m³
            return (d * b * 25) / 1000000; // Convert mm² to m²
        }
        static double DesignLoad(double deadLoad, double liveLoad, double selfWeight)
        {
            // Total load on the beam considering partial safety factors
            return 1.4 * (deadLoad + selfWeight) + 1.6 * liveLoad;

        }
        // design bending moment
        static double Moment(double totalLoad, double spanLength)
        {
            // Assuming a simply supported beam with a uniformly distributed load
            return (totalLoad * spanLength * spanLength) / 8; // kNm
        }
        // design shear force
        static double ShearForce(double totalLoad, double spanLength)
        {
            // Assuming a simply supported beam with a uniformly distributed load
            return (totalLoad * spanLength) / 2; // kN
        }
        // Calculate the ultimate moment capacity of the beam
        static double momentCapacity(double fck, double b, double d_eff)
        {
            // Assuming a rectangular section and using the formula for ultimate moment capacity
            // M_u = 0.138 * fck * b * d^2
            return (0.156 * fck * b * d_eff * d_eff) / 1000000; // kNm
        }
        // calculate k
        static double CalculateK(double designMoment, double selectedConcrete, double b, double d_eff)
        {
            return (designMoment * 1000000) / (selectedConcrete * b * d_eff * d_eff);
        }
        // lever arm 
        static double CalculateLeverArm(double d_eff, double K)
        {
            return d_eff * (0.5 + Math.Sqrt(0.25 - (K / 0.9)));
        }
        // Calculate the area of steel reinforcement required
        static double CalculateofAst(double designMoment, double selectedmainSteel, double leverArm)
        {
            // Using the formula Ast = M_d / (0.87 * fy * leverArm)
            return designMoment * 1000000 / (0.87 * selectedmainSteel * leverArm); // in mm²
        }
        // no of bars
        static double CalculateNumberOfBars(double Ast, double phi)
        {
            // Area of one bar = π * (φ/2)²
            double areaOfOneBar = Math.PI * Math.Pow((phi / 2), 2);
            return Math.Ceiling(Ast / areaOfOneBar); // Round up to the nearest whole number
        }
        //Ast provided
        static double CalulateAstProvided(double numberofBars, double phi)
        {
            return numberofBars * (Math.PI * Math.Pow(phi / 2, 2)); // in mm²
        }


        // shear stress
        static double UltimateShearStress(double designShearForce, double b, double d_eff)
        {
            return (designShearForce * 1000) / (b * d_eff); // in MPa
        }
        //percentage of shear reinforcement
        static double percentageShear(double Ast_provdided, double b, double d_eff)
        {
            return (100 * Ast_provdided) / (b * d_eff); // in MPa
        }
        // design concrete shear stress
        static double DesignConcreteShearStress(double selectedConcrete, double adjustmentFactor)
        {
            // Using the formula for design shear stress
            return Math.Pow(selectedConcrete / 25, 1 / 3) * adjustmentFactor; // in N/mm²
        }
        //shear reinforcement check
        static double ShearCheck(double designShearStress, double designConcreteShearStress)
        {
            return 0.7 + designConcreteShearStress; // in N/mm²
        }
        // if tau is less than tau_c + 0.4
        static double shearcheck1(double b, double selectedSecondarySteel)
        {
            return (0.4 * b) / (0.87 * selectedSecondarySteel);
        }
        // if tau is greater than tau_c + 0.4
        static double shearcheck2(double b, double selectedSecondarySteel, double tau, double tau_c)
        {
            return (b * (tau - tau_c)) / (0.87 * selectedSecondarySteel);
        }
        // deflection check
        static double spantodepth(double spanLength, double d_eff)
        {
            return spanLength / d_eff; // in mm
        }
        static double Mbd(double designMoment, double b, double d_eff) //Table 3.10, BS 8110
        {
            return (designMoment * 1000000) / (b * d_eff * d_eff * d_eff); 
        }
        static double f_s(double selectedmainSteel, double Ast, double Ast_provided)
        {
            // Calculate the stress in the main steel reinforcement
            return ((5 /8) * (selectedmainSteel) * Ast) / Ast_provided; // in N/mm²
        }
        //deflection check
        static double PermissibleDeflection(double modificationFactor)
        {
            // Assuming a permissible deflection of 20 times the effective depth
            return 20 * modificationFactor; // in mm
        }
    }
}














