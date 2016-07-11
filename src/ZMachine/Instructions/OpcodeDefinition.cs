using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZMachine.Instructions
{
    public struct OpcodeDefinition
    {
        public bool IsCall;
        public string Name;
        public OpcodeIdentifier ID;
        public bool HasStore;
        public bool HasBranch;
        public bool HasText
        {
            get
            {
                return (ID.OperandCount == OperandCountType.OP0) &&
                        (ID.OpcodeNumber == 178 || ID.OpcodeNumber == 179);
            }
        }

        public static OpcodeDefinition GetKnownOpcode(OpcodeIdentifier id)
        {
            try
            {
                return _knownOpcodes.Where(x => x.ID.Equals(id)).First();
            }
            catch
            {
                return OpcodeDefinition.InvalidOpcode;
            }
        }

        public override string ToString()
        {
            return Name + " " + ID.ToString();
        }

        public static readonly OpcodeDefinition InvalidOpcode = new OpcodeDefinition() { Name = "Invalid" };

        /// <summary>
        /// All opcodes defined in the spec 14.1
        /// </summary>
        private static OpcodeDefinition[] _knownOpcodes { get; } = new OpcodeDefinition[]
        {
            /////////////////////////
            // Double operand opcodes 
            /////////////////////////

            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_1"),  HasStore = false, HasBranch = true,  Name ="je", } ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_2"),  HasStore = false, HasBranch = true,  Name ="jl", } ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_3"),  HasStore = false, HasBranch = true,  Name ="jg", } ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_4"),  HasStore = false, HasBranch = true,  Name ="dec_chk", } ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_5"),  HasStore = false, HasBranch = true,  Name ="inc_chk", } ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_6"),  HasStore = false, HasBranch = true,  Name ="jin", } ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_7"),  HasStore = false, HasBranch = true,  Name ="test", } ,

            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_8"),  HasStore = true,  HasBranch = false, Name ="or", } ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_9"),  HasStore = true,  HasBranch = false, Name ="and",} ,

            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_10"), HasStore = false, HasBranch = true,  Name ="test_attr" ,} ,

            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_11"), HasStore = false, HasBranch = false, Name ="set_attr", } ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_12"), HasStore = false, HasBranch = false, Name ="clear_attr",} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_13"), HasStore = false, HasBranch = false, Name ="store", } ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_14"), HasStore = false, HasBranch = false, Name ="insert_obj", } ,

            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_15"), HasStore = true,  HasBranch = false, Name ="loadw", } ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_16"), HasStore = true,  HasBranch = false, Name ="loadb", } ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_17"), HasStore = true,  HasBranch = false, Name ="get_prop", } ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_18"), HasStore = true,  HasBranch = false, Name ="get_prop_addr", } ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_19"), HasStore = true,  HasBranch = false, Name ="get_prop_prop", } ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_20"), HasStore = true,  HasBranch = false, Name ="add", } ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_21"), HasStore = true,  HasBranch = false, Name ="sub", } ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_22"), HasStore = true,  HasBranch = false, Name ="mul", } ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_23"), HasStore = true,  HasBranch = false, Name ="div", } ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_24"), HasStore = true,  HasBranch = false, Name ="mod", } ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_25"), IsCall=true, HasStore = true,  HasBranch = false, Name ="call_s2", } ,

            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_26"), IsCall=true, HasStore = false, HasBranch = false, Name ="call_2n", } ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_27"), HasStore = false, HasBranch = false, Name ="set_colour", } ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP2_28"), HasStore = false, HasBranch = false, Name ="throw", } ,

            /////////////////////////
            // Single operand opcodes 
            /////////////////////////


            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP1_128"), HasStore = false, HasBranch = true,  Name ="jz" ,} ,

            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP1_129"), HasStore = true,  HasBranch = true,  Name ="get_sibling" ,} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP1_130"), HasStore = true,  HasBranch = true,  Name ="get_child" ,} ,

            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP1_131"), HasStore = true,  HasBranch = false, Name ="get_parent" ,} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP1_132"), HasStore = true,  HasBranch = false, Name ="get_prop_len" ,} ,

            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP1_133"), HasStore = false, HasBranch = false, Name ="inc" ,} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP1_134"), HasStore = false, HasBranch = false, Name ="dec" ,} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP1_135"), HasStore = false, HasBranch = false, Name ="print_addr" ,} ,

            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP1_136"), IsCall=true, HasStore = true,  HasBranch = false, Name ="call_1s" ,} ,

            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP1_137"), HasStore = false, HasBranch = false, Name ="remove_obj" ,} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP1_138"), HasStore = false, HasBranch = false, Name ="print_obj" ,} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP1_139"), HasStore = false, HasBranch = false, Name ="ret" ,} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP1_140"), HasStore = false, HasBranch = false, Name ="jump" ,} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP1_141"), HasStore = false, HasBranch = false, Name ="print_paddr" ,} ,

            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP1_142"), HasStore = true,  HasBranch = false, Name ="load" ,} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP1_143"), IsCall=true, HasStore = true,  HasBranch = false, Name ="not" ,} ,   // v1-4
            // new OpcodeDefinition{ ID = new OpcodeIdentifier("OP1_143"), HasStore = false, HasBranch = false, Name ="call_1n" ,} , // v5

            ///////////////////////
            // Zero operand opcodes 
            ///////////////////////

            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP0_176"), HasStore = false, HasBranch = false, Name ="rtrue" ,} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP0_177"), HasStore = false, HasBranch = false, Name ="rfalse" ,} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP0_178"), HasStore = false, HasBranch = false, Name ="print" ,} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP0_179"), HasStore = false, HasBranch = false, Name ="print_ret" ,} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP0_180"), HasStore = false, HasBranch = false, Name ="nop" ,} ,

            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP0_181"), HasStore = false, HasBranch = true,  Name ="save" ,} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP0_182"), HasStore = false, HasBranch = true,  Name ="restore" ,} ,

            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP0_183"), HasStore = false, HasBranch = false,  Name ="restart" ,} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP0_184"), HasStore = false, HasBranch = false,  Name ="ret_popped" ,} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP0_185"), HasStore = false, HasBranch = false,  Name ="pop" ,} ,
            //new OpcodeDefinition{ ID = new OpcodeIdentifier("OP0_185"), HasStore = true, HasBranch = false,  Name ="catch" ,} , // v5-6
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP0_186"), HasStore = false, HasBranch = false,  Name ="quit" ,} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP0_187"), HasStore = false, HasBranch = false,  Name ="new_line" ,} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP0_188"), HasStore = false, HasBranch = false,  Name ="show_status" ,} ,  // v3
            //new OpcodeDefinition{ ID = new OpcodeIdentifier("OP0_188"), HasStore = false, HasBranch = false,  Name ="illegal" ,} ,  // v4

            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP0_189"), HasStore = false, HasBranch = true,  Name ="verify" ,} , // v3

            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP0_190"), HasStore = false, HasBranch = false,  Name ="extended" ,} , // v5

            new OpcodeDefinition{ ID = new OpcodeIdentifier("OP0_191"), HasStore = false, HasBranch = true,  Name ="piracy" ,} , // v5

            ///////////////////////////
            // Variable operand opcodes
            ///////////////////////////

            new OpcodeDefinition{ ID = new OpcodeIdentifier("VAR_224"), IsCall=true, HasStore = true,  HasBranch = false, Name ="call" ,} , // v1
            //new OpcodeDefinition{ ID = new OpcodeIdentifier("VAR_224"), HasStore = false, HasBranch = false, Name ="call_vs" ,} , // v4

            new OpcodeDefinition{ ID = new OpcodeIdentifier("VAR_225"), HasStore = false, HasBranch = false, Name ="storew" ,} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("VAR_226"), HasStore = false, HasBranch = false, Name ="storeb" ,} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("VAR_227"), HasStore = false, HasBranch = false, Name ="put_prop" ,} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("VAR_228"), HasStore = false, HasBranch = false, Name ="sread" ,} ,  // v1
            //new OpcodeDefinition{ ID = new OpcodeIdentifier("VAR_228"), HasStore = false, HasBranch = false, Name ="sread" ,} ,  // v4
            //new OpcodeDefinition{ ID = new OpcodeIdentifier("VAR_228"), HasStore = true,  HasBranch = false, Name ="sread" ,} ,  // v5
            new OpcodeDefinition{ ID = new OpcodeIdentifier("VAR_229"), HasStore = false, HasBranch = false, Name ="print_char" ,} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("VAR_230"), HasStore = false, HasBranch = false, Name ="print_num" ,} ,

            new OpcodeDefinition{ ID = new OpcodeIdentifier("VAR_231"), HasStore = true,  HasBranch = false, Name ="random" ,} ,

            new OpcodeDefinition{ ID = new OpcodeIdentifier("VAR_232"), HasStore = false, HasBranch = false, Name ="push" ,} ,
            new OpcodeDefinition{ ID = new OpcodeIdentifier("VAR_233"), HasStore = false, HasBranch = false, Name ="pull" ,} ,  // v1
            //new OpcodeDefinition{ ID = new OpcodeIdentifier("VAR_233"), HasStore = true,  HasBranch = false, Name ="pull" ,} ,  // v6

            new OpcodeDefinition{ ID = new OpcodeIdentifier("VAR_234"), HasStore = false, HasBranch = false, Name ="split_window" ,} , // v3
            new OpcodeDefinition{ ID = new OpcodeIdentifier("VAR_235"), HasStore = false, HasBranch = false, Name ="set_window" ,} , // v3

            new OpcodeDefinition{ ID = new OpcodeIdentifier("VAR_236"), HasStore = true, HasBranch = false, Name ="call_vs2" ,} , // v4

            // (skipping some post v3 opcodes here)

            new OpcodeDefinition{ ID = new OpcodeIdentifier("VAR_243"), HasStore = false, HasBranch = false, Name ="output_stream" ,} , // v3

            new OpcodeDefinition{ ID = new OpcodeIdentifier("VAR_244"), HasStore = false, HasBranch = false, Name ="input_stream" ,} , // v3
            new OpcodeDefinition{ ID = new OpcodeIdentifier("VAR_245"), HasStore = false, HasBranch = false, Name ="sound_effect" ,} , // v5/3

            /////////////////////////////////////////////
            // Extended opcodes currently omitted because
            // they are only valid for v5-6
            /////////////////////////////////////////////
        };
    }
}
