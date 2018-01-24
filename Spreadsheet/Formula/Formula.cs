﻿// Skeleton written by Joe Zachary for CS 3500, January 2017

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Formulas
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  Provides a means to evaluate Formulas.  Formulas can be composed of
    /// non-negative floating-point numbers, variables, left and right parentheses, and
    /// the four binary operator symbols +, -, *, and /.  (The unary operators + and -
    /// are not allowed.)
    /// </summary>
    public class Formula
    {

        //Object Variables
        Stack<String> valuesStack;
        Stack<String> OperatorStack;

        String lpPattern = @"^\($";
        String rpPattern = @"\)$";
        String opPattern = @"[\+\-*/]$";
        String varPattern = @"^[a-zA-Z][0-9a-zA-Z]*$";
        String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: e[\+-]?\d+)?";
        IEnumerable<String> formulaStrings;

        Boolean hasToken = false;
        Boolean firstToken = false;
        Boolean lastToken = false;
        Boolean followOpeningOperator = false;
        Boolean followNumberVariableClosing = false;
        int lpCount = 0;
        int rpCount = 0;

        /// <summary>
        /// Creates a Formula from a string that consists of a standard infix expression composed
        /// from non-negative floating-point numbers (using C#-like syntax for double/int literals), 
        /// variable symbols (a letter followed by zero or more letters and/or digits), left and right
        /// parentheses, and the four binary operator symbols +, -, *, and /.  White space is
        /// permitted between tokens, but is not required.
        /// 
        /// Examples of a valid parameter to this constructor are:
        ///     "2.5e9 + x5 / 17"
        ///     "(5 * 2) + 8"
        ///     "x*y-2+35/9"
        ///     
        /// Examples of invalid parameters are:
        ///     "_"
        ///     "-5.3"
        ///     "2 5 + 3"
        /// 
        /// If the formula is syntacticaly invalid, throws a FormulaFormatException with an 
        /// explanatory Message.
        /// </summary>
        public Formula(String formula)
        {
            this.formulaStrings  = GetTokens(formula);

            //Iterator that loops through each token.
            foreach (String token in formulaStrings)
            {
                //If the token is recognized as valid. (Valid tokens are described in the Formula class. and code is provided to detect them.)
                if (Regex.IsMatch(token, doublePattern, RegexOptions.IgnorePatternWhitespace) ||
                    Regex.IsMatch(token, varPattern) || Regex.IsMatch(token, opPattern) ||
                    Regex.IsMatch(token, rpPattern) || Regex.IsMatch(token, lpPattern))
                {
                    //Change boolean to reflect presence of at least one token.
                    this.hasToken = true;

                    //The first token of a formula must be a number, a variable, or an opening parenthesis.
                    //Check for the first token.  If it is not a number, variable or an opening parenthesis, throw exception.
                    if (this.firstToken == false)
                    {
                        if (Regex.IsMatch(token, doublePattern, RegexOptions.IgnorePatternWhitespace) ||
                           Regex.IsMatch(token, varPattern) || Regex.IsMatch(token, lpPattern))
                        {
                            this.firstToken = true;
                        }
                        else
                        {
                            throw new FormulaFormatException("First token is not a number, variable or opening parenthesis");
                        }
                    }

                    //The last token of a formula must be a number, a variable, or a closing parenthesis.
                    //If the last viewed token is a right parentheses, variable or a number, then it could be a valid last token.
                    if (Regex.IsMatch(token, rpPattern) || Regex.IsMatch(token, varPattern) ||
                        Regex.IsMatch(token, doublePattern, RegexOptions.IgnorePatternWhitespace))
                    {
                        this.lastToken = true;
                    }
                    else
                    {
                        //If the last token seen is not a valid last token, then the iteration will end with an error.
                        this.lastToken = false;
                    }
                    if(this.followOpeningOperator == true)
                    {
                        if(Regex.IsMatch(token, doublePattern, RegexOptions.IgnorePatternWhitespace) ||
                           Regex.IsMatch(token, varPattern) || Regex.IsMatch(token, lpPattern))
                        {
                            this.followOpeningOperator = false;
                        }
                        else
                        {
                            throw new FormulaFormatException("The token following an opening parenthesis or an oporator is not valid");
                        }
                    }




                    //Any token that immediately follows an opening parenthesis or an operator must be either a number, a variable, or an opening parenthesis.
                    if(Regex.IsMatch(token, lpPattern) || Regex.IsMatch(token,opPattern))
                    {
                        this.followOpeningOperator = true;
                    }
                    else
                    {
                        this.followOpeningOperator = false;
                    }






                    if(this.followNumberVariableClosing == true)
                    {
                        if(Regex.IsMatch(token, opPattern) || Regex.IsMatch(token, rpPattern))
                        {
                            this.followNumberVariableClosing = false;
                        }
                        else
                        {
                            throw new FormulaFormatException("The token following a number, variable or closing parenthesis is not valid");
                        }
                    }

                    //Any token that immediately follows a number, a variable, or a closing parenthesis must be either an operator or a closing parenthesis.
                    if(Regex.IsMatch(token, doublePattern, RegexOptions.IgnorePatternWhitespace) ||
                        Regex.IsMatch(token, varPattern) || Regex.IsMatch(token, rpPattern))
                    {
                        this.followNumberVariableClosing = true;
                    }
                    else
                    {
                        this.followNumberVariableClosing = false;
                    }



                    //When reading tokens from left to right, at no point should the number of closing parentheses seen so far be greater than the number of opening parentheses seen so far.
                    if (Regex.IsMatch(token, lpPattern))
                    {
                        this.lpCount++;
                    }

                    if (Regex.IsMatch(token, rpPattern))
                    {
                        this.rpCount++;
                    }
                    
                    if(this.rpCount > this.lpCount)
                    {
                        throw new FormulaFormatException("Closing parentheses appear before openining parentheses.");
                    }




                }
                else
                {
                    //Throw a format exception.
                    throw new FormulaFormatException(token + " is an invalid input");
                }
            }

            //If no tokens are present
            if (hasToken == false)
            {
                throw new FormulaFormatException("There must be at least one token");
            }

            //If the last token is not a valid last token, throw an exception.
            if(this.lastToken == false)
            {
                throw new FormulaFormatException("The last token is not a valid last input");
            }

            //The total number of opening parentheses must equal the total number of closing parentheses.
            if (this.rpCount != this.lpCount)
            {
                throw new FormulaFormatException("The number of opening and closing parentheses is not equal");
            }




            
           


        }
        /// <summary>
        /// Evaluates this Formula, using the Lookup delegate to determine the values of variables.  (The
        /// delegate takes a variable name as a parameter and returns its value (if it has one) or throws
        /// an UndefinedVariableException (otherwise).  Uses the standard precedence rules when doing the evaluation.
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, its value is returned.  Otherwise, throws a FormulaEvaluationException  
        /// with an explanatory Message.
        /// </summary>
        public double Evaluate(Lookup lookup)
        {
            return 0;
        }

        /// <summary>
        /// Given a formula, enumerates the tokens that compose it.  Tokens are left paren,
        /// right paren, one of the four operator symbols, a string consisting of a letter followed by
        /// zero or more digits and/or letters, a double literal, and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens.
            // NOTE:  These patterns are designed to be used to create a pattern to split a string into tokens.
            // For example, the opPattern will match any string that contains an operator symbol, such as
            // "abc+def".  If you want to use one of these patterns to match an entire string (e.g., make it so
            // the opPattern will match "+" but not "abc+def", you need to add ^ to the beginning of the pattern
            // and $ to the end (e.g., opPattern would need to be @"^[\+\-*/]$".)
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z][0-9a-zA-Z]*";

            // PLEASE NOTE:  I have added white space to this regex to make it more readable.
            // When the regex is used, it is necessary to include a parameter that says
            // embedded white space should be ignored.  See below for an example of this.
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: e[\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern.  It contains embedded white space that must be ignored when
            // it is used.  See below for an example of this.  This pattern is useful for 
            // splitting a string into tokens.
            String splittingPattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            // PLEASE NOTE:  Notice the second parameter to Split, which says to ignore embedded white space
            /// in the pattern.
            foreach (String s in Regex.Split(formula, splittingPattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }
        }
    }

    /// <summary>
    /// A Lookup method is one that maps some strings to double values.  Given a string,
    /// such a function can either return a double (meaning that the string maps to the
    /// double) or throw an UndefinedVariableException (meaning that the string is unmapped 
    /// to a value. Exactly how a Lookup method decides which strings map to doubles and which
    /// don't is up to the implementation of the method.
    /// </summary>
    public delegate double Lookup(string var);

    /// <summary>
    /// Used to report that a Lookup delegate is unable to determine the value
    /// of a variable.
    /// </summary>
    [Serializable]
    public class UndefinedVariableException : Exception
    {
        /// <summary>
        /// Constructs an UndefinedVariableException containing whose message is the
        /// undefined variable.
        /// </summary>
        /// <param name="variable"></param>
        public UndefinedVariableException(String variable)
            : base(variable)
        {
        }
    }

    /// <summary>
    /// Used to report syntactic errors in the parameter to the Formula constructor.
    /// </summary>
    [Serializable]
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message) : base(message)
        {
        }
    }

    /// <summary>
    /// Used to report errors that occur when evaluating a Formula.
    /// </summary>
    [Serializable]
    public class FormulaEvaluationException : Exception
    {
        /// <summary>
        /// Constructs a FormulaEvaluationException containing the explanatory message.
        /// </summary>
        public FormulaEvaluationException(String message) : base(message)
        {
        }
    }
}
