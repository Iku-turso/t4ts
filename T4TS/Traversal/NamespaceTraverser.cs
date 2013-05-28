﻿using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T4TS
{
    public class NamespaceTraverser
    {
        public Action<CodeClass> WithCodeClass { get; private set; }

        public NamespaceTraverser(CodeNamespace ns, Action<CodeClass> withCodeClass)
        {
            if (ns == null)
                throw new ArgumentNullException("ns");
            
            if (withCodeClass == null)
                throw new ArgumentNullException("withCodeClass");
            
            WithCodeClass = withCodeClass;
            
            if (ns.Members != null)
                Traverse(ns.Members);
        }

        private void Traverse(CodeElements members)
        {
            foreach (var codeClass in members.OfType<CodeClass>())
                Traverse(codeClass);
        }

        /// <summary>
        /// Traverse recursively to the baseclasses first, as they are likely to contain types used elsewhere. Kludge: there is some unnecessary recurring traverses of same base-classes here.
        /// </summary>
        /// <param name="codeClass"></param>
        private void Traverse(CodeClass codeClass)
        {
            foreach (var memberCodeClass in codeClass.Bases.OfType<CodeClass>())
            {
                Traverse(memberCodeClass);
            }
            WithCodeClass(codeClass);
        }
    }
}
