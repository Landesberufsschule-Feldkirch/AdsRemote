using AdsRemote;
using AdsRemote.Common;

namespace LAP_2010_2_Transportwagen.Cx9020
{
    public class Kommunikation
    {
        /************************************************************************************
         *                              AdsInput 
         ************************************************************************************/

        [LinkedTo("AdsInput.B1")]
        public Var<bool> B1 { get; set; }       // Endschalter Links

        [LinkedTo("AdsInput.B2")]
        public Var<bool> B2 { get; set; }       // Endschalter Links

        [LinkedTo("AdsInput.F1")]
        public Var<bool> F1 { get; set; }       // Thermorelais

        [LinkedTo("AdsInput.S1")]
        public Var<bool> S1 { get; set; }       // Taster "Start"

        [LinkedTo("AdsInput.S2")]
        public Var<bool> S2 { get; set; }       // NotHalt

        [LinkedTo("AdsInput.S3")]
        public Var<bool> S3 { get; set; }       // Taster Reset


        /*************************************************************************************
        *                              AdsOutput 
        *************************************************************************************/


        [LinkedTo("AdsOutput.P1")]
        public Var<bool> P1 { get; set; }       // Störung

        [LinkedTo("AdsOutput.Q1")]
        public Var<bool> Q1 { get; set; }       // Motor LL

        [LinkedTo("AdsOutput.Q2")]
        public Var<bool> Q2 { get; set; }       // Motor RL

    }
}
