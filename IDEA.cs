using System;

namespace MainHomework
{
    class IDEA : System.Security.Cryptography.SymmetricAlgorithm, System.Security.Cryptography.ICryptoTransform
    {
        protected const uint multi_modul = (1 << 16) + 1;
        protected const uint modulo = 1 << 16;
        protected const uint mod = 65537;
        public const int BLOCK_SIZE = 4096;

        #region comment
        /*uint[] key = new uint[] {
            0xFEFA, 0xFBFD, 0xFBAC, 0xFAAC, 0xDDDD, 0xFFAA, 0xDFAA, 0xFFAC
        };*/
        #endregion

        
      

        uint[] subkey_for_encrypt = new uint[52];
        uint[] subkey_for_decrypt = new uint[52];
        uint[] subkey = new uint[52];


        
        private void SubkeyGeneration(byte[] key)
        {
            uint iterator = 0;
            uint flag = 8;
            uint num_of_iter = 25;
            uint[] buf_int = new uint[8];
            
            for (uint i = 0; i < key.Length/2; i++)
            {
                
                Array.Clear(buf_int, 0, buf_int.Length);

                uint jndex = 0; 
                for (uint ind = 0; ind < key.Length & iterator < subkey_for_encrypt.Length; ind += 2)
                {
                    buf_int[jndex] = (buf_int[jndex] | key[ind]) << 8;
                    subkey_for_encrypt[iterator++] = buf_int[jndex++] | key[ind + 1];
                 
                }
                
                if (iterator == subkey_for_encrypt.Length)
                    break;

                num_of_iter = 25;
                for (uint j = 0; j < num_of_iter; j++)
                {
                   
                    for (uint ind = 0; ind < key.Length; ind++)
                    {
                        if ((key[ind] & 128) == 0)
                        {
                            key[ind] <<= 1;
                            if (ind == (key.Length - 1) & flag == 1)
                            {
                                flag = 0;
                                key[ind] |= 1;
                            }
                        }
                        else if (ind == 0)
                        {
                            flag = 1;
                            key[ind] <<= 1;
                        }
                        else
                        {
                            key[ind] <<= 1;
                            key[ind - 1] |= 1;
                            if (ind == (key.Length - 1) & flag == 1)
                            {
                                flag = 0;
                                key[ind] |= 1;
                            }
                        }
                    }
                }
            }

           ReverseSubKey();

           #region output
            /*
           for (uint i = 0; i < subkey_for_encrypt.Length; i++)
            {
               
                Console.WriteLine("{0,3:x2}", (short)subkey_for_encrypt[i]);
                if ((i + 1) % 8 == 0)
                    Console.WriteLine();
            }
             * */
           #endregion output
        }
        

        
        private void ReverseSubKey()
        {
            uint iterator = 0;
            uint flag = 4;
           
          
            for (uint i = 0; i < subkey_for_encrypt.Length; i += 3)
            {
                subkey_for_decrypt[i] = EuclidAlgorithm(subkey_for_encrypt[subkey_for_encrypt.Length - flag + i]);
                iterator++;
                if ((iterator & 1) == 0)
                    flag += 12;
            }
            flag = 6;
            for (uint i = 4; i <= 46; i += 6)
            {
                subkey_for_decrypt[i] = subkey_for_encrypt[subkey_for_encrypt.Length - flag];
                subkey_for_decrypt[i + 1] = subkey_for_encrypt[subkey_for_encrypt.Length - flag + 1];
                iterator += 2;
                flag += 6;
            }
            subkey_for_decrypt[1] = mod - 1 - subkey_for_encrypt[subkey_for_encrypt.Length - 3];
            subkey_for_decrypt[2] = mod - 1 - subkey_for_encrypt[subkey_for_encrypt.Length - 2];
            subkey_for_decrypt[subkey_for_decrypt.Length - 3] = mod - 1 - subkey_for_encrypt[1];
            subkey_for_decrypt[subkey_for_decrypt.Length - 2] = mod - 1 - subkey_for_encrypt[2];
            iterator += 4;
            for (uint i = 7; i < 48; i += 6)
            {
                subkey_for_decrypt[i] = mod - 1 - subkey_for_encrypt[subkey_for_encrypt.Length - i - 1];
                subkey_for_decrypt[i + 1] = mod - 1 - subkey_for_encrypt[subkey_for_encrypt.Length - i - 2];
                iterator += 2;
            }
            if (iterator != 52)
                Console.WriteLine("damn code");

            #region output
            /*
            for (uint i = 0; i < subkey_for_decrypt.Length; i++)
            {
                Console.WriteLine("{0,3:x2}", (short)subkey_for_decrypt[i]);
                if ((i + 1) % 8 == 0)
                    Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine();
             */
            #endregion output
        }



        private uint EuclidAlgorithm(uint b) // x*a - y*b = 1
        {
            
            uint a = mod;
            uint r, remainder, x, y;
            uint x1 = 0, x2 = 1, y1 = 1, y2 = 0;
            
            while (b > 0) 
            {
                remainder = a / b;
                r = a - remainder * b;

                x = x2 - remainder * x1; 
                y = y2 - remainder * y1;

                a = b;
                b = r;

                x2 = x1; 
                x1 = x; 
                y2 = y1; 
                y1 = y;
            
           }
           return (mod + y2) % mod;

        }   

 
       



        private void IDEA_Crypt(uint[] data)
        {
            CryptMode mode = crypt_mode;
            if (mode == CryptMode.Encrypt)
                subkey = subkey_for_encrypt;
            else if (mode == CryptMode.Decrypt)
                subkey = subkey_for_decrypt; 

            Console.WriteLine();
            
            
            uint k = 0;
            uint ind;
            for (uint num_of_block = 0; num_of_block < data.Length; num_of_block+=4)
            {
                ind = num_of_block;
                k = 0;

                for (uint i = 0; i < 8; i++)
                {
                    ind = num_of_block;
                    uint A = (subkey[k++] * data[ind++]) % multi_modul; 
                    uint B = (subkey[k++] + data[ind++]) % modulo; 
                    uint C = (subkey[k++] + data[ind++]) % modulo; 
                    uint D = (subkey[k++] * data[ind]) % multi_modul; 
                    uint E = A ^ C;
                    uint F = B ^ D;
                    ind -=3 ;

                    if (subkey[k] == 0)
                        subkey[k] = 65536;
                    if (subkey[k + 1] == 0)
                        subkey[k + 1] = 65536;

                    uint val = (E * subkey[k]) % multi_modul;
                    
                    long val_end = (long)((long)(((long)(F + val) % modulo) * subkey[k + 1]) % multi_modul);
                    
                    data[ind++] = (uint)(A ^ val_end);
                    data[ind++] = (uint)(C ^ val_end);

                    val_end = ((long)((F + val) % modulo) * subkey[k + 1]) % multi_modul;
                   

                    data[ind++] = (uint)(B ^ ((val + val_end) % modulo));
                    data[ind++] = (uint)(D ^ ((val + val_end) % modulo));
                    k += 2;

                    #region output
                    /*for (uint p = 0; p < 4; p++)
                    {
                        Console.WriteLine("{0,3:x2}", data[p]);
                        if (p == 3)
                            Console.WriteLine();
                    }*/
                    #endregion output

                }
                ind = num_of_block;
                uint dat;
                
                data[ind] = (data[ind] * subkey[k++]) % multi_modul;
                ++ind;  
                    
                dat = data[ind];
                data[ind] = (data[ind + 1] + subkey[k++]) % modulo;
                ++ind;
                data[ind++] = (dat + subkey[k++]) % modulo;
                data[ind] = (data[ind] * subkey[k]) % multi_modul;

                #region output
                /*for (uint p = 0; p < 4; p++)
                     Console.WriteLine("{0,3:x2}", data[p]);
                  */
                #endregion output

            }
        }

        CryptMode crypt_mode;
        byte[] crypt_key;

        public IDEA(CryptMode pmode, byte[] pkey)
        {
            crypt_mode = pmode;
            crypt_key = pkey;
            SubkeyGeneration(pkey);

        }

        public override System.Security.Cryptography.ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            return new IDEA(CryptMode.Decrypt, rgbKey);
        }

        public override System.Security.Cryptography.ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            return new IDEA(CryptMode.Encrypt, rgbKey);
        }

        public override void GenerateIV()
        {
           
        }

        public override void GenerateKey()
        {
            crypt_key = new byte[16];
            System.Random rnd = new System.Random();
            rnd.NextBytes(crypt_key);
        }

        bool System.Security.Cryptography.ICryptoTransform.CanReuseTransform
        {
            get { return false; }
        }

        bool System.Security.Cryptography.ICryptoTransform.CanTransformMultipleBlocks
        {
            get { return true; }
        }

        int System.Security.Cryptography.ICryptoTransform.InputBlockSize
        {
            get { return BLOCK_SIZE; }
        }

        int System.Security.Cryptography.ICryptoTransform.OutputBlockSize
        {
            get { return BLOCK_SIZE; }
        }

        int Transform(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) {
            uint[] data = new uint[((inputCount + 1) / 2 + 3) / 4 * 4];
            for (int i = inputOffset; i < inputOffset + inputCount; ++i)
            {
                int k = (i - inputOffset) / 2;
                if ((i - inputOffset) % 2 == 0)
                {
                    data[k] |= inputBuffer[i];
                }
                else
                    data[k] |= ((uint)inputBuffer[i] << 8);
            }
            IDEA_Crypt(data);
            for (int i = 0, k = outputOffset; i < data.Length; ++i, k+=2)
            {
                outputBuffer[k] = (byte)(data[i] & 0xff);
                outputBuffer[k + 1] = (byte)(data[i] >> 8);
            }
            return data.Length * 2;
           }

        int System.Security.Cryptography.ICryptoTransform.TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            return Transform(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }

        byte[] System.Security.Cryptography.ICryptoTransform.TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            byte[] outputBuffer = new byte[(inputCount + 7) / 8 * 8];
            Transform(inputBuffer, inputOffset, inputCount, outputBuffer, 0);
            return outputBuffer;
        }

        void IDisposable.Dispose()
        {
        }
    }

}