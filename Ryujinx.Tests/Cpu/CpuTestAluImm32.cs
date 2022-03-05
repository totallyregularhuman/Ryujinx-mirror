#define AluRs32

using NUnit.Framework;
using System.Runtime.CompilerServices;

namespace Ryujinx.Tests.Cpu
{
    [Category("AluImm32")]
    public sealed class CpuTestAluImm32 : CpuTest32
    {
#if AluRs32

#region "ValueSource (Opcodes)"
        private static uint[] _Adc_Adcs_Rsc_Rscs_Sbc_Sbcs_()
        {
            return new uint[]
            {
                0xe2a00000u, // ADC R0, R0, #0
                0xe2b00000u, // ADCS R0, R0, #0
                0xe2e00000u, // RSC R0, R0, #0
                0xe2f00000u, // RSCS R0, R0, #0
                0xe2c00000u, // SBC R0, R0, #0
                0xe2d00000u  // SBCS R0, R0, #0
            };
        }
#endregion

        private const int RndCnt = 2;
        private const int RndCntAmount = 2;

        [Test, Pairwise]
        public void Adc_Adcs_Rsc_Rscs_Sbc_Sbcs([ValueSource("_Adc_Adcs_Rsc_Rscs_Sbc_Sbcs_")] uint opcode,
                                               [Values(0u, 13u)] uint rd,
                                               [Values(1u, 13u)] uint rn,
                                               [Random(RndCnt)] uint imm,
                                               [Random(RndCnt)] uint wn,
                                               [Values(true, false)] bool carryIn)
        {
            opcode |= ((imm & 0xfff) << 0) | ((rn & 15) << 16) | ((rd & 15) << 12);

            uint sp = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, r1: wn, sp: sp, carry: carryIn);

            CompareAgainstUnicorn();
        }
#endif
    }
}
