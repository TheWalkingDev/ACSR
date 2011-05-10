

using System.Collections.Generic;
using System.Text;

namespace ACSR.Core.Strings
{
    public class StringSplitter
    {
        private List<string> _strings;
        private char _delimiter = ',';
        private char _quote = '"';

        public List<string> Lines
        {
            get
            {
                return _strings;
            }
        }
        public StringSplitter()
        {
            _strings = new List<string>();
        }

        private void Split_old(string AString)
        {
            _strings.Clear();
            int quoteCnt = 0;
            StringBuilder sb = new StringBuilder();
            bool inQuote;
            for (int i=0;i < AString.Length; i++)
            {
                char c = AString[i];
                if (c == _quote)
                {
                    quoteCnt++;
                    inQuote = quoteCnt % 2 == 0;
                    if (inQuote)
                    {
                        if (i < AString.Length - 1 && AString[i + 1] == _quote)
                            sb.Append(c);
                        continue;
                    }


                }
                else
                    inQuote = quoteCnt % 2 == 0;
                if (c == _delimiter)
                {
                    if (inQuote)
                    {
                        sb.Append(c);
                    }
                    else
                    {
                        _strings.Add(sb.ToString());
                        sb = new StringBuilder();
                        quoteCnt = 0;
                    }
                }
                else
                {
                    if (inQuote || quoteCnt == 0)
                    {
                        sb.Append(c);
                    }
                }

            }
        }


        public void Split_broken(string AString)
        {
            _strings.Clear();
            char nullChar = new char();
            if (_delimiter == new byte())
            {
                _strings.Add(AString);
                return;
            }

            

            char lDelim  = nullChar;
            int lStrPos  = 0;
            int lQuotCnt = 0;
            int lBufLen  = 0;
            int lLen = AString.Length-1;
            StringBuilder sb = new StringBuilder();
  
  
            while (lStrPos <= lLen-1)
            {
                char lNextChar = AString[lStrPos];
                if (lNextChar == _quote)
                    lQuotCnt ++;
                // True if quotes are closed, in other words NOT inside
                bool lMod = (lQuotCnt%2 == 0);

                bool lIsToken = false;
                // Build Delimiter
                if (lMod)
                {
                    if (_delimiter == lNextChar)
                    {
                        lStrPos++;
                        lIsToken = true;
                    }

                }

                // Next phase is to check whether the current character is a quote
                // if so then check the next character, and if that's a quote as well
                // and currently we are not inside quotes, then add to token, if not then
                // if we are INSIDE quotes then simply add, else check whether the delimiter
                // is matched through IsToken.
                if (lNextChar == _quote)
                {
                    if (lStrPos < lLen)
                    {
                        if (AString[lStrPos + 1] == _quote && lMod)

                            sb.Append(lNextChar);
                    }
                    // reset to 0 if equal quotes
                    if (lMod) lQuotCnt = 0;
                }
                else
                {
                    if (!lMod)
                    {
                        sb.Append(lNextChar);
                    }
                    else if (lQuotCnt == 0)
                    {
                        if
                            (!lIsToken)
                            sb.Append(lNextChar);
                    }
                }

                if (lIsToken || (lStrPos >= lLen-1))
                {
                    if (lMod || (lStrPos >= lLen-1))
                    {
                        _strings.Add(sb.ToString());

                        sb = new StringBuilder();
                        lBufLen = 0;
                        lDelim = nullChar;
                        lQuotCnt = 0;
                    }
                }

                lStrPos ++;
            }

        }

        public void Split_test(string AString)
        {

            int lStrPos;
            int lQuotCnt;
            int lLen;
            int lDelimLen;
            int lBufLen;
            string lTokBuf;
            string lToken;
            char lDelim;
            bool lMod;
            bool lIsToken;
            char lNextChar;



            Lines.Clear();
            char nullChar = new char();
            if (_delimiter == nullChar)
            {
                Lines.Add(AString);
                return;
            }

            StringBuilder sb = new StringBuilder();
            lDelim = nullChar;
            lStrPos = 0;
            lQuotCnt = 0;
            lBufLen = 0;
            lLen = AString.Length - 1; // Length (iValue);
            //SetLength(lTokBuf, lLen+1);
            //lDelimLen := Length (FDelimeter);
            while (lStrPos <= lLen)
            {
                lNextChar = AString[lStrPos];
                if (lNextChar == _quote)
                    lQuotCnt++;
                // True if quotes are closed, in other words NOT inside
                lMod = (lQuotCnt%2 == 0);

                lIsToken = false;
                // Build Delimiter
                if (lMod)
                {

                    if (_delimiter == lNextChar)
                    {



                        //lStrPos = lStrPos + lDelimLen - 1;
                        lIsToken = true;


                    }

                }

                // Next phase is to check whether the current character is a quote
                // if so then check the next character, and if that's a quote as well
                // and currently we are not inside quotes, then add to token, if not then
                // if we are INSIDE quotes then simply add, else check whether the delimiter
                // is matched through IsToken.
                if (lNextChar == _quote)
                {
                    if (lStrPos < lLen)
                    {
                        if ((AString[lStrPos + 1] == _quote) && lMod)
                            sb.Append(lNextChar);
                    }
                    // reset to 0 if equal quotes
                    if (lMod)
                        lQuotCnt = 0;
                }
                else
                {
                    if (!lMod)
                    {
                        sb.Append(lNextChar);
                    }
                    else if (lQuotCnt == 0)
                    {
                        if
                            (!lIsToken)
                            sb.Append(lNextChar);
                    }
                }

                if ((lIsToken) || (lStrPos >= lLen))
                {
                    if (lMod || (lStrPos >= lLen))
                    {

                        Lines.Add(sb.ToString());
                        sb = new StringBuilder();
                        lBufLen = 0;
                        lDelim = nullChar;
                        lQuotCnt = 0;
                    }
                }

                lStrPos++;
            }
            sb = new StringBuilder();

            lDelim = nullChar;
        }

    }

 }