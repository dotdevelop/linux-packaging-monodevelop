using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Mono.Debugging.Client;
using Mono.Debugging.Backend;
using System.Diagnostics;
using System.Collections;

namespace Mono.Debugging.Evaluation
{
	public abstract class ObjectValueAdaptor: IDisposable
	{
		Dictionary<string, TypeDisplayData> typeDisplayData = new Dictionary<string, TypeDisplayData> ();

		// Time to wait while evaluating before switching to async mode
		public int DefaultEvaluationWaitTime { get; set; }
		
		public event EventHandler<BusyStateEventArgs> BusyStateChanged;
		
		AsyncEvaluationTracker asyncEvaluationTracker = new AsyncEvaluationTracker ();
		AsyncOperationManager asyncOperationManager = new AsyncOperationManager ();
		static Dictionary<string, string> netToCSharpTypes = new Dictionary<string, string> ();

		static ObjectValueAdaptor ()
		{
			netToCSharpTypes["System.Void"]    = "void";
			netToCSharpTypes["System.Object"]  = "object";
			netToCSharpTypes["System.Boolean"] = "bool";
			netToCSharpTypes["System.Byte"]    = "byte";
			netToCSharpTypes["System.SByte"]   = "sbyte";
			netToCSharpTypes["System.Char"]    = "char";
			netToCSharpTypes["System.Enum"]    = "enum";
			netToCSharpTypes["System.Int16"]   = "short";
			netToCSharpTypes["System.Int32"]   = "int";
			netToCSharpTypes["System.Int64"]   = "long";
			netToCSharpTypes["System.UInt16"]  = "ushort";
			netToCSharpTypes["System.UInt32"]  = "uint";
			netToCSharpTypes["System.UInt64"]  = "ulong";
			netToCSharpTypes["System.Single"]  = "float";
			netToCSharpTypes["System.Double"]  = "double";
			netToCSharpTypes["System.Decimal"] = "decimal";
			netToCSharpTypes["System.String"]  = "string";
		}
		
		public ObjectValueAdaptor ()
		{
			DefaultEvaluationWaitTime = 100;
			
			asyncOperationManager.BusyStateChanged += delegate(object sender, BusyStateEventArgs e) {
				OnBusyStateChanged (e);
			};
			asyncEvaluationTracker.WaitTime = DefaultEvaluationWaitTime;
		}
		
		public void Dispose ()
		{
			asyncEvaluationTracker.Dispose ();
			asyncOperationManager.Dispose ();
		}

		public ObjectValue CreateObjectValue (EvaluationContext ctx, IObjectValueSource source, ObjectPath path, object obj, ObjectValueFlags flags)
		{
			try {
				return CreateObjectValueImpl (ctx, source, path, obj, flags);
			}
			catch (Exception ex) {
				Console.WriteLine (ex);
				return ObjectValue.CreateFatalError (path.LastName, ex.Message, flags);
			}
		}
		
		public virtual string GetDisplayTypeName (string typeName)
		{
			return GetDisplayTypeName (typeName.Replace ('+','.'), 0, typeName.Length);
		}
		
		public string GetDisplayTypeName (EvaluationContext ctx, object type)
		{
			return GetDisplayTypeName (GetTypeName (ctx, type));
		}
		
		string GetDisplayTypeName (string typeName, int idx, int end)
		{
			int i = typeName.IndexOf ('[', idx, end - idx);  // Bracket may be an array start or a generic arg definition start
			int ci = typeName.IndexOf (',', idx, end - idx); // Text after comma is the assembly name
			
			List<string> genericArgs = null;
			string array = string.Empty;
			int te = end;
			if (i != -1) te = i;
			if (ci != -1 && ci < te) te = ci;
			
			if (i != -1 && typeName.IndexOf ('`', idx, te - idx) != -1) {
				// Is generic
				genericArgs = GetGenericArguments (typeName, ref i);
				if (i >= end || typeName [i] != '[')
					i = -1;
			}
			if (i != -1) {
				// Is array
				while (i < end && typeName [i] == '[') {
					int ea = typeName.IndexOf (']', i);
					array += typeName.Substring (i, ea - i + 1);
					i = ea + 1;
				}
			}
			
			if (genericArgs == null)
				return GetShortTypeName (typeName.Substring (idx, te - idx)) + array;

			// Insert the generic arguments next to each type.
			// for example: Foo`1+Bar`1[System.Int32,System.String]
			// is converted to: Foo<int>.Bar<string>
			StringBuilder sb = new StringBuilder ();
			int gi = 0;
			int j = typeName.IndexOf ('`', idx, te - idx);
			while (j != -1) {
				sb.Append (typeName.Substring (idx, j - idx)).Append ('<');
				int ej = ++j;
				while (ej < typeName.Length && char.IsDigit (typeName [ej]))
					ej++;
				int n;
				if (int.TryParse (typeName.Substring (j, ej - j), out n)) {
					while (n > 0 && gi < genericArgs.Count) {
						sb.Append (genericArgs [gi++]);
						if (--n > 0)
							sb.Append (',');
					}
				}
				sb.Append ('>');
				idx = ej;
				j = typeName.IndexOf ('`', idx, te - idx);
			}
			sb.Append (typeName.Substring (idx, te - idx)).Append (array);
			return sb.ToString ();
		}
		
		List<string> GetGenericArguments (string typeName, ref int i)
		{
			// Get a list of the generic arguments.
			// When returning, i points to the next char after the closing ']'
			List<string> genericArgs = new List<string> ();
			i++;
			while (i < typeName.Length && typeName [i] != ']') {
				int pend = FindTypeEnd (typeName, i);
				bool escaped = typeName [i] == '[';
				genericArgs.Add (GetDisplayTypeName (typeName, escaped ? i + 1 : i, escaped ? pend - 1 : pend));
				i = pend;
				if (i < typeName.Length && typeName[i] == ',')
					i++;
			}
			i++;
			return genericArgs;
		}
		
		int FindTypeEnd (string s, int i)
		{
			int bc = 0;
			while (i < s.Length) {
				char c = s[i];
				if (c == '[')
					bc++;
				else if (c == ']') {
					if (bc > 0)
						bc--;
					else
						return i;
				}
				else if (c == ',' && bc == 0)
					return i;
				i++;
			}
			return i;
		}
		
		public virtual string GetShortTypeName (string tname)
		{
			string res;
			if (netToCSharpTypes.TryGetValue (tname, out res))
				return res;
			else
				return tname;
		}
		
		public virtual void OnBusyStateChanged (BusyStateEventArgs e)
		{
			EventHandler<BusyStateEventArgs> evnt = BusyStateChanged;
			if (evnt != null)
				evnt (this, e);
		}

		public abstract ICollectionAdaptor CreateArrayAdaptor (EvaluationContext ctx, object arr);

		public abstract bool IsNull (EvaluationContext ctx, object val);
		public abstract bool IsPrimitive (EvaluationContext ctx, object val);
		public abstract bool IsArray (EvaluationContext ctx, object val);
		public abstract bool IsEnum (EvaluationContext ctx, object val);
		public abstract bool IsClass (object type);
		public abstract object TryCast (EvaluationContext ctx, object val, object type);

		public abstract object GetValueType (EvaluationContext ctx, object val);
		public abstract string GetTypeName (EvaluationContext ctx, object val);
		public abstract object[] GetTypeArgs (EvaluationContext ctx, object type);
		public abstract object GetBaseType (EvaluationContext ctx, object type);
		
		public virtual bool IsFlagsEnumType (EvaluationContext ctx, object type)
		{
			return true;
		}
		
		public virtual IEnumerable<EnumMember> GetEnumMembers (EvaluationContext ctx, object type)
		{
			object longType = GetType (ctx, "System.Int64");
			TypeValueReference tref = new TypeValueReference (ctx, type);
			foreach (ValueReference cr in tref.GetChildReferences (ctx.Options)) {
				object c = TryCast (ctx, cr.Value, longType);
				if (c == null)
					continue;
				long val = (long) TargetObjectToObject (ctx, c);
				EnumMember em = new EnumMember () { Name = cr.Name, Value = val };
				yield return em;
			}
		}
		
		public object GetBaseType (EvaluationContext ctx, object type, bool includeObjectClass)
		{
			object bt = GetBaseType (ctx, type);
			string tn = GetTypeName (ctx, bt);
			if (!includeObjectClass && bt != null && (tn == "System.Object" || tn == "System.ValueType"))
				return null;
			else
				return bt;
		}

		public virtual bool IsClassInstance (EvaluationContext ctx, object val)
		{
			return IsClass (GetValueType (ctx, val));
		}
		
		public virtual bool IsExternalType (EvaluationContext ctx, object type)
		{
			return false;
		}
		
		public object GetType (EvaluationContext ctx, string name)
		{
			return GetType (ctx, name, null);
		}

		public abstract object GetType (EvaluationContext ctx, string name, object[] typeArgs);

		public virtual string GetValueTypeName (EvaluationContext ctx, object val)
		{
			return GetTypeName (ctx, GetValueType (ctx, val));
		}

		public virtual object CreateTypeObject (EvaluationContext ctx, object type)
		{
			return default (object);
		}
		
		public virtual object ForceLoadType (EvaluationContext ctx, string typeName)
		{
			return GetType (ctx, typeName);
		}

		public abstract object CreateValue (EvaluationContext ctx, object value);

		public abstract object CreateValue (EvaluationContext ctx, object type, params object[] args);

		public abstract object CreateNullValue (EvaluationContext ctx, object type);

		public virtual object GetBaseValue (EvaluationContext ctx, object val)
		{
			return val;
		}

		public virtual string[] GetImportedNamespaces (EvaluationContext ctx)
		{
			return new string[0];
		}

		public virtual void GetNamespaceContents (EvaluationContext ctx, string namspace, out string[] childNamespaces, out string[] childTypes)
		{
			childTypes = childNamespaces = new string[0];
		}

		protected virtual ObjectValue CreateObjectValueImpl (EvaluationContext ctx, Mono.Debugging.Backend.IObjectValueSource source, ObjectPath path, object obj, ObjectValueFlags flags)
		{
			string typeName = obj != null ? GetValueTypeName (ctx, obj) : "";
			
			if (obj == null || IsNull (ctx, obj)) {
				return ObjectValue.CreateNullObject (source, path, GetDisplayTypeName (typeName), flags);
			}
			else if (IsPrimitive (ctx, obj) || IsEnum (ctx,obj)) {
				return ObjectValue.CreatePrimitive (source, path, GetDisplayTypeName (typeName), ctx.Evaluator.TargetObjectToExpression (ctx, obj), flags);
			}
			else if (IsArray (ctx, obj)) {
				return ObjectValue.CreateObject (source, path, GetDisplayTypeName (typeName), ctx.Evaluator.TargetObjectToExpression (ctx, obj), flags, null);
			}
			else {
				TypeDisplayData tdata = GetTypeDisplayData (ctx, GetValueType (ctx, obj));
				
				EvaluationResult tvalue;
				if (!string.IsNullOrEmpty (tdata.ValueDisplayString) && ctx.Options.AllowDisplayStringEvaluation)
					tvalue = new EvaluationResult (EvaluateDisplayString (ctx, obj, tdata.ValueDisplayString));
				else
					tvalue = ctx.Evaluator.TargetObjectToExpression (ctx, obj);
				
				string tname;
				if (!string.IsNullOrEmpty (tdata.TypeDisplayString) && ctx.Options.AllowDisplayStringEvaluation)
					tname = EvaluateDisplayString (ctx, obj, tdata.TypeDisplayString);
				else
					tname = GetDisplayTypeName (typeName);
				
				ObjectValue oval = ObjectValue.CreateObject (source, path, tname, tvalue, flags, null);
				if (!string.IsNullOrEmpty (tdata.NameDisplayString) && ctx.Options.AllowDisplayStringEvaluation)
					oval.Name = EvaluateDisplayString (ctx, obj, tdata.NameDisplayString);
				return oval;
			}
		}

		public ObjectValue[] GetObjectValueChildren (EvaluationContext ctx, IObjectSource objectSource, object obj, int firstItemIndex, int count)
		{
			return GetObjectValueChildren (ctx, objectSource, GetValueType (ctx, obj), obj, firstItemIndex, count, true);
		}

		public virtual ObjectValue[] GetObjectValueChildren (EvaluationContext ctx, IObjectSource objectSource, object type, object obj, int firstItemIndex, int count, bool dereferenceProxy)
		{
			if (IsArray (ctx, obj)) {
				ArrayElementGroup agroup = new ArrayElementGroup (ctx, CreateArrayAdaptor (ctx, obj));
				return agroup.GetChildren (ctx.Options);
			}

			if (IsPrimitive (ctx, obj))
				return new ObjectValue[0];

			bool showRawView = false;
			
			// If there is a proxy, it has to show the members of the proxy
			object proxy = obj;
			if (dereferenceProxy) {
				proxy = GetProxyObject (ctx, obj);
				if (proxy != obj) {
					type = GetValueType (ctx, proxy);
					showRawView = true;
				}
			}

			TypeDisplayData tdata = GetTypeDisplayData (ctx, type);
			bool groupPrivateMembers = ctx.Options.GroupPrivateMembers && (ctx.Options.GroupUserPrivateMembers || IsExternalType (ctx, type));

			List<ObjectValue> values = new List<ObjectValue> ();
			BindingFlags flattenFlag = ctx.Options.FlattenHierarchy ? (BindingFlags)0 : BindingFlags.DeclaredOnly;
			BindingFlags nonNonPublicFlag = groupPrivateMembers || showRawView ? (BindingFlags)0 : BindingFlags.NonPublic;
			BindingFlags staticFlag = ctx.Options.GroupStaticMembers ? (BindingFlags)0 : BindingFlags.Static;
			BindingFlags access = BindingFlags.Public | BindingFlags.Instance | flattenFlag | nonNonPublicFlag | staticFlag;
			
			// Load all members to a list before creating the object values,
			// to avoid problems with objects being invalidated due to evaluations in the target,
			List<ValueReference> list = new List<ValueReference> ();
			list.AddRange (GetMembersSorted (ctx, objectSource, type, proxy, access));
			
			object tdataType = type;
			
			foreach (ValueReference val in list) {
				try {
					object decType = val.DeclaringType;
					if (decType != null && decType != tdataType) {
						tdataType = decType;
						tdata = GetTypeDisplayData (ctx, decType);
					}
					DebuggerBrowsableState state = tdata.GetMemberBrowsableState (val.Name);
					if (state == DebuggerBrowsableState.Never)
						continue;

					if (state == DebuggerBrowsableState.RootHidden && dereferenceProxy) {
						object ob = val.Value;
						if (ob != null) {
							values.Clear ();
							values.AddRange (GetObjectValueChildren (ctx, val, ob, -1, -1));
							showRawView = true;
							break;
						}
					}
					else {
						ObjectValue oval = val.CreateObjectValue (true);
						values.Add (oval);
					}

				}
				catch (Exception ex) {
					ctx.WriteDebuggerError (ex);
					values.Add (ObjectValue.CreateError (null, new ObjectPath (val.Name), GetDisplayTypeName (GetTypeName (ctx, val.Type)), ex.Message, val.Flags));
				}
			}

			if (showRawView) {
				values.Add (RawViewSource.CreateRawView (ctx, objectSource, obj));
			}
			else {
				if (IsArray (ctx, proxy)) {
					ICollectionAdaptor col = CreateArrayAdaptor (ctx, proxy);
					ArrayElementGroup agroup = new ArrayElementGroup (ctx, col);
					ObjectValue val = ObjectValue.CreateObject (null, new ObjectPath ("Raw View"), "", "", ObjectValueFlags.ReadOnly, values.ToArray ());
					values = new List<ObjectValue> ();
					values.Add (val);
					values.AddRange (agroup.GetChildren (ctx.Options));
				}
				else {
					if (ctx.Options.GroupStaticMembers && HasMembers (ctx, type, proxy, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | flattenFlag)) {
						access = BindingFlags.Static | BindingFlags.Public | flattenFlag | nonNonPublicFlag;
						values.Add (FilteredMembersSource.CreateStaticsNode (ctx, objectSource, type, proxy, access));
					}
					if (groupPrivateMembers && HasMembers (ctx, type, proxy, BindingFlags.Instance | BindingFlags.NonPublic | flattenFlag | staticFlag))
						values.Add (FilteredMembersSource.CreateNonPublicsNode (ctx, objectSource, type, proxy, BindingFlags.Instance | BindingFlags.NonPublic | flattenFlag | staticFlag));
					
					if (!ctx.Options.FlattenHierarchy) {
						object baseType = GetBaseType (ctx, type, false);
						if (baseType != null)
							values.Insert (0, BaseTypeViewSource.CreateBaseTypeView (ctx, objectSource, baseType, proxy));
					}
				}
			}
			return values.ToArray ();
		}

		public ObjectValue[] GetExpressionValuesAsync (EvaluationContext ctx, string[] expressions)
		{
			ObjectValue[] values = new ObjectValue[expressions.Length];
			for (int n = 0; n < values.Length; n++) {
				string exp = expressions[n];
				// This is a workaround to a bug in mono 2.0. That mono version fails to compile
				// an anonymous method here
				ExpData edata = new ExpData (ctx, exp, this);
				values[n] = asyncEvaluationTracker.Run (exp, ObjectValueFlags.Literal, edata.Run);
			}
			return values;
		}
		
		class ExpData
		{
			public EvaluationContext ctx;
			public string exp;
			public ObjectValueAdaptor adaptor;
			
			public ExpData (EvaluationContext ctx, string exp, ObjectValueAdaptor adaptor)
			{
				this.ctx = ctx;
				this.exp = exp;
				this.adaptor = adaptor;
			}
			
			public ObjectValue Run ()
			{
				return adaptor.GetExpressionValue (ctx, exp);
			}
		}

		public virtual ValueReference GetIndexerReference (EvaluationContext ctx, object target, object[] indices)
		{
			return null;
		}

		public ValueReference GetLocalVariable (EvaluationContext ctx, string name)
		{
			return OnGetLocalVariable (ctx, name);
		}

		protected virtual ValueReference OnGetLocalVariable (EvaluationContext ctx, string name)
		{
			ValueReference best = null;
			foreach (ValueReference var in GetLocalVariables (ctx)) {
				if (var.Name == name)
					return var;
				if (!ctx.Evaluator.CaseSensitive && var.Name.Equals (name, StringComparison.CurrentCultureIgnoreCase))
					best = var;
			}
			return best;
		}

		public virtual ValueReference GetParameter (EvaluationContext ctx, string name)
		{
			return OnGetParameter (ctx, name);
		}

		protected virtual ValueReference OnGetParameter (EvaluationContext ctx, string name)
		{
			ValueReference best = null;
			foreach (ValueReference var in GetParameters (ctx)) {
				if (var.Name == name)
					return var;
				if (!ctx.Evaluator.CaseSensitive && var.Name.Equals (name, StringComparison.CurrentCultureIgnoreCase))
					best = var;
			}
			return best;
		}

		public IEnumerable<ValueReference> GetLocalVariables (EvaluationContext ctx)
		{
			return OnGetLocalVariables (ctx);
		}

		public ValueReference GetThisReference (EvaluationContext ctx)
		{
			return OnGetThisReference (ctx);
		}

		public IEnumerable<ValueReference> GetParameters (EvaluationContext ctx)
		{
			return OnGetParameters (ctx);
		}

		protected virtual IEnumerable<ValueReference> OnGetLocalVariables (EvaluationContext ctx)
		{
			yield break;
		}

		protected virtual IEnumerable<ValueReference> OnGetParameters (EvaluationContext ctx)
		{
			yield break;
		}

		protected virtual ValueReference OnGetThisReference (EvaluationContext ctx)
		{
			return null;
		}

		public virtual ValueReference GetCurrentException (EvaluationContext ctx)
		{
			return null;
		}

		public virtual object GetEnclosingType (EvaluationContext ctx)
		{
			return null;
		}

		public virtual CompletionData GetExpressionCompletionData (EvaluationContext ctx, string exp)
		{
			int i;
			if (exp.Length == 0)
				return null;

			if (exp [exp.Length - 1] == '.') {
				exp = exp.Substring (0, exp.Length - 1);
				i = 0;
				while (i < exp.Length) {
					ValueReference vr = null;
					try {
						vr = ctx.Evaluator.Evaluate (ctx, exp.Substring (i), null);
						if (vr != null) {
							CompletionData data = new CompletionData ();
							foreach (ValueReference cv in vr.GetChildReferences (ctx.Options))
								data.Items.Add (new CompletionItem (cv.Name, cv.Flags));
							data.ExpressionLenght = 0;
							return data;
						}
					} catch (Exception ex) {
						Console.WriteLine (ex);
					}
					i++;
				}
				return null;
			}
			
			i = exp.Length - 1;
			bool lastWastLetter = false;
			while (i >= 0) {
				char c = exp [i--];
				if (!char.IsLetterOrDigit (c) && c != '_')
					break;
				lastWastLetter = !char.IsDigit (c);
			}
			if (lastWastLetter) {
				string partialWord = exp.Substring (i+1);
				
				CompletionData data = new CompletionData ();
				data.ExpressionLenght = partialWord.Length;
				
				// Local variables
				
				foreach (ValueReference vc in GetLocalVariables (ctx))
					if (vc.Name.StartsWith (partialWord))
						data.Items.Add (new CompletionItem (vc.Name, vc.Flags));
				
				// Parameters
				
				foreach (ValueReference vc in GetParameters (ctx))
					if (vc.Name.StartsWith (partialWord))
						data.Items.Add (new CompletionItem (vc.Name, vc.Flags));
				
				// Members
				
				ValueReference thisobj = GetThisReference (ctx);
				
				if (thisobj != null)
					data.Items.Add (new CompletionItem ("this", ObjectValueFlags.Field | ObjectValueFlags.ReadOnly));

				object type = GetEnclosingType (ctx);
				
				foreach (ValueReference vc in GetMembers (ctx, null, type, thisobj != null ? thisobj.Value : null))
					if (vc.Name.StartsWith (partialWord))
						data.Items.Add (new CompletionItem (vc.Name, vc.Flags));
				
				if (data.Items.Count > 0)
					return data;
			}
			return null;
		}
		
		public IEnumerable<ValueReference> GetMembers (EvaluationContext ctx, IObjectSource objectSource, object t, object co)
		{
			foreach (ValueReference val in GetMembers (ctx, t, co, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)) {
				val.ParentSource = objectSource;
				yield return val;
			}
		}

		public ValueReference GetMember (EvaluationContext ctx, IObjectSource objectSource, object co, string name)
		{
			return GetMember (ctx, objectSource, GetValueType (ctx, co), co, name);
		}

		public ValueReference GetMember (EvaluationContext ctx, IObjectSource objectSource, object t, object co, string name)
		{
			ValueReference m = GetMember (ctx, t, co, name);
			if (m != null)
				m.ParentSource = objectSource;
			return m;
		}
		
		protected virtual ValueReference GetMember (EvaluationContext ctx, object t, object co, string name)
		{
			ValueReference best = null;
			foreach (ValueReference var in GetMembers (ctx, t, co, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)) {
				if (var.Name == name)
					return var;
				if (!ctx.Evaluator.CaseSensitive && var.Name.Equals (name, StringComparison.CurrentCultureIgnoreCase))
					best = var;
			}
			return best;
		}

		internal IEnumerable<ValueReference> GetMembersSorted (EvaluationContext ctx, IObjectSource objectSource, object t, object co)
		{
			return GetMembersSorted (ctx, objectSource, t, co, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
		}
		
		internal IEnumerable<ValueReference> GetMembersSorted (EvaluationContext ctx, IObjectSource objectSource, object t, object co, BindingFlags bindingFlags)
		{
			List<ValueReference> list = new List<ValueReference> ();
			foreach (ValueReference vr in GetMembers (ctx, t, co, bindingFlags)) {
				vr.ParentSource = objectSource;
				list.Add (vr);
			}
			list.Sort (delegate (ValueReference v1, ValueReference v2) {
				return v1.Name.CompareTo (v2.Name);
			});
			return list;
		}
		
		public bool HasMembers (EvaluationContext ctx, object t, object co, BindingFlags bindingFlags)
		{
			return GetMembers (ctx, t, co, bindingFlags).Any ();
		}
		
		/// <summary>
		/// Returns all members of a type. The following binding flags have to be honored:
		/// BindingFlags.Static, BindingFlags.Instance, BindingFlags.Public, BindingFlags.NonPublic, BindingFlags.DeclareOnly
		/// </summary>
		protected abstract IEnumerable<ValueReference> GetMembers (EvaluationContext ctx, object t, object co, BindingFlags bindingFlags);
		
		public virtual IEnumerable<object> GetNestedTypes (EvaluationContext ctx, object type)
		{
			yield break;
		}
		
		public virtual object CreateArray (EvaluationContext ctx, object type, object[] values)
		{
			object arrType = GetType (ctx, "System.Collections.ArrayList");
			object arrayList = CreateValue (ctx, arrType, new object[0]);
			object[] objTypes = new object[] { GetType (ctx, "System.Object") };
			foreach (object value in values)
				RuntimeInvoke (ctx, arrType, arrayList, "Add", objTypes, new object[] { value });
			
			object typof = CreateTypeObject (ctx, type);
			objTypes = new object[] { GetType (ctx, "System.Type") };
			return RuntimeInvoke (ctx, arrType, arrayList, "ToArray", objTypes, new object[] { typof });
		}
		
		public virtual object ToRawValue (EvaluationContext ctx, IObjectSource source, object obj)
		{
			if (IsEnum (ctx, obj)) {
				object longType = GetType (ctx, "System.Int64");
				object c = Cast (ctx, obj, longType);
				return TargetObjectToObject (ctx, c);
			}
			if (IsPrimitive (ctx, obj))
				return TargetObjectToObject (ctx, obj);
				
			if (IsArray (ctx, obj)) {
				ICollectionAdaptor adaptor = CreateArrayAdaptor (ctx, obj);
				return new RawValueArray (new RemoteRawValueArray (ctx, source, adaptor, obj));
			}
			return new RawValue (new RemoteRawValue (ctx, source, obj));
		}
		
		public virtual object FromRawValue (EvaluationContext ctx, object obj)
		{
			if (obj is RawValue) {
				RemoteRawValue val = ((RawValue)obj).Source as RemoteRawValue;
				if (val == null)
					throw new InvalidOperationException ("Unknown RawValue source: " + ((RawValue)obj).Source);
				return val.TargetObject;
			}
			else if (obj is RawValueArray) {
				RemoteRawValueArray val = ((RawValueArray)obj).Source as RemoteRawValueArray;
				if (val == null)
					throw new InvalidOperationException ("Unknown RawValue source: " + ((RawValueArray)obj).Source);
				return val.TargetObject;
			}
			else {
				if (obj is Array) {
					Array arr = (Array) obj;
					if (obj.GetType ().GetElementType () == typeof(RawValue)) {
						throw new NotSupportedException ();
					} else {
						object elemType = GetType (ctx, obj.GetType ().GetElementType ().FullName);
						if (elemType == null)
							throw new EvaluatorException ("Unknown target type: {0}", obj.GetType ().GetElementType ().FullName);
						object[] values = new object [arr.Length];
						for (int n=0; n<values.Length; n++)
							values [n] = FromRawValue (ctx, arr.GetValue (n));
						return CreateArray (ctx, elemType, values);
					}
				}
				return CreateValue (ctx, obj);
			}
		}
		
		public virtual object TargetObjectToObject (EvaluationContext ctx, object obj)
		{
			if (IsNull (ctx, obj)) {
				return null;
			} else if (IsArray (ctx, obj)) {
				ICollectionAdaptor adaptor = CreateArrayAdaptor (ctx, obj);
				string ename = GetDisplayTypeName (GetTypeName (ctx, adaptor.ElementType));
				int[] dims = adaptor.GetDimensions ();
				StringBuilder tn = new StringBuilder ("[");
				for (int n=0; n<dims.Length; n++) {
					if (n>0)
						tn.Append (',');
					tn.Append (dims[n]);
				}
				tn.Append ("]");
				int i = ename.LastIndexOf ('>');
				if (i == -1) i = 0;
				i = ename.IndexOf ('[', i);
				if (i != -1)
					return new EvaluationResult ("{" + ename.Substring (0, i) + tn + ename.Substring (i) + "}");
				else
					return new EvaluationResult ("{" + ename + tn + "}");
			}
			else if (IsEnum (ctx, obj)) {
				object type = GetValueType (ctx, obj);
				object longType = GetType (ctx, "System.Int64");
				object c = Cast (ctx, obj, longType);
				long val = (long) TargetObjectToObject (ctx, c);
				long rest = val;
				string typeName = GetTypeName (ctx, type);
				string composed = string.Empty;
				string composedDisplay = string.Empty;
				foreach (EnumMember em in GetEnumMembers (ctx, type)) {
					if (em.Value == val)
						return new EvaluationResult (typeName + "." + em.Name, em.Name);
					else {
						if (em.Value != 0 && (rest & em.Value) == em.Value) {
							rest &= ~em.Value;
							if (composed.Length > 0) {
								composed += "|";
								composedDisplay += "|";
							}
							composed += typeName + "." + em.Name;
							composedDisplay += em.Name;
						}
					}
				}
				if (IsFlagsEnumType (ctx, type) && rest == 0 && composed.Length > 0)
					return new EvaluationResult (composed, composedDisplay);
				else
					return new EvaluationResult (val.ToString ());
			}
			else if (GetValueTypeName (ctx, obj) == "System.Decimal") {
				string res = CallToString (ctx, obj);
				// This returns the decimal formatted using the current culture. It has to be converted to invariant culture.
				decimal dec = decimal.Parse (res);
				res = dec.ToString (System.Globalization.CultureInfo.InvariantCulture);
				return new EvaluationResult (res);
			}
			else if (IsClassInstance (ctx, obj)) {
				TypeDisplayData tdata = GetTypeDisplayData (ctx, GetValueType (ctx, obj));
				if (!string.IsNullOrEmpty (tdata.ValueDisplayString) && ctx.Options.AllowDisplayStringEvaluation)
					return new EvaluationResult (EvaluateDisplayString (ctx, obj, tdata.ValueDisplayString));
				// Return the type name
				if (ctx.Options.AllowToStringCalls)
					return new EvaluationResult ("{" + CallToString (ctx, obj) + "}");
				if (!string.IsNullOrEmpty (tdata.TypeDisplayString) && ctx.Options.AllowDisplayStringEvaluation)
					return new EvaluationResult ("{" + EvaluateDisplayString (ctx, obj, tdata.TypeDisplayString) + "}");
				return new EvaluationResult ("{" + GetDisplayTypeName (GetValueTypeName (ctx, obj)) + "}");
			}
			return new EvaluationResult ("{" + CallToString (ctx, obj) + "}");
		}

		public object Convert (EvaluationContext ctx, object obj, object targetType)
		{
			if (obj == null)
				return null;
			object res = TryConvert (ctx, obj, targetType);
			if (res != null)
				return res;
			else
				throw new EvaluatorException ("Can't convert an object of type '{0}' to type '{1}'", GetValueTypeName (ctx, obj), GetTypeName (ctx, targetType));
		}

		public virtual object TryConvert (EvaluationContext ctx, object obj, object targetType)
		{
			return TryCast (ctx, obj, targetType);
		}

		public virtual object Cast (EvaluationContext ctx, object obj, object targetType)
		{
			if (obj == null)
				return null;
			object res = TryCast (ctx, obj, targetType);
			if (res != null)
				return res;
			else
				throw new EvaluatorException ("Can't cast an object of type '{0}' to type '{1}'", GetValueTypeName (ctx, obj), GetTypeName (ctx, targetType));
		}

		public virtual string CallToString (EvaluationContext ctx, object obj)
		{
			return GetValueTypeName (ctx, obj);
		}

		public object GetProxyObject (EvaluationContext ctx, object obj)
		{
			TypeDisplayData data = GetTypeDisplayData (ctx, GetValueType (ctx, obj));
			if (string.IsNullOrEmpty (data.ProxyType) || !ctx.Options.AllowDebuggerProxy)
				return obj;

			object[] typeArgs = null;

			int i = data.ProxyType.IndexOf ('`');
			if (i != -1) {
				// The proxy type is an uninstantiated generic type.
				// The number of type args of the proxy must match the args of the target object
				int j = i + 1;
				for (; j < data.ProxyType.Length && char.IsDigit (data.ProxyType[j]); j++);
				int np = int.Parse (data.ProxyType.Substring (i + 1, j - i - 1));
				typeArgs = GetTypeArgs (ctx, GetValueType (ctx, obj));
				if (typeArgs.Length != np)
					return obj;
			}
			
			object ttype = GetType (ctx, data.ProxyType, typeArgs);
			if (ttype == null) {
				i = data.ProxyType.IndexOf (',');
				if (i != -1)
					ttype = GetType (ctx, data.ProxyType.Substring (0, i).Trim (), typeArgs);
			}
			if (ttype == null)
				throw new EvaluatorException ("Unknown type '{0}'", data.ProxyType);

			try {
				object val = CreateValue (ctx, ttype, obj);
				return val ?? obj;
			} catch (Exception ex) {
				ctx.WriteDebuggerError (ex);
				return obj;
			}
		}

		public TypeDisplayData GetTypeDisplayData (EvaluationContext ctx, object type)
		{
			if (!IsClass (type))
				return TypeDisplayData.Default;

			TypeDisplayData td;
			string tname = GetTypeName (ctx, type);
			if (typeDisplayData.TryGetValue (tname, out td))
				return td;

			try {
				td = OnGetTypeDisplayData (ctx, type);
			}
			catch (Exception ex) {
				Console.WriteLine (ex);
			}
			if (td == null)
				typeDisplayData[tname] = td = TypeDisplayData.Default;
			else
				typeDisplayData[tname] = td;
			return td;
		}

		protected virtual TypeDisplayData OnGetTypeDisplayData (EvaluationContext ctx, object type)
		{
			return null;
		}

		public string EvaluateDisplayString (EvaluationContext ctx, object obj, string exp)
		{
			StringBuilder sb = new StringBuilder ();
			int last = 0;
			int i = exp.IndexOf ("{");
			while (i != -1 && i < exp.Length) {
				sb.Append (exp.Substring (last, i - last));
				i++;
				int j = exp.IndexOf ("}", i);
				if (j == -1)
					return exp;
				string mem = exp.Substring (i, j - i).Trim ();
				if (mem.Length == 0)
					return exp;
				
				string[] props = mem.Split (new char[] { '.' });
				ValueReference member = null;
				object val = obj;
				
				for (int k = 0; k < props.Length; k++) {
					member = GetMember (ctx, null, GetValueType (ctx, val), val, props[k]);
					if (member == null)
						break;
					
					val = member.Value;
				}
				
				if (member != null) {
					sb.Append (ctx.Evaluator.TargetObjectToString (ctx, val));
				} else {
					sb.Append ("{Unknown member '" + mem + "'}");
				}
				last = j + 1;
				i = exp.IndexOf ("{", last);
			}
			sb.Append (exp.Substring (last));
			return sb.ToString ();
		}

		public void AsyncExecute (AsyncOperation operation, int timeout)
		{
			asyncOperationManager.Invoke (operation, timeout);
		}

		public ObjectValue CreateObjectValueAsync (string name, ObjectValueFlags flags, ObjectEvaluatorDelegate evaluator)
		{
			return asyncEvaluationTracker.Run (name, flags, evaluator);
		}
		
		public bool IsEvaluating {
			get { return asyncEvaluationTracker.IsEvaluating; }
		}

		public void CancelAsyncOperations ( )
		{
			asyncEvaluationTracker.Stop ();
			asyncOperationManager.AbortAll ();
			asyncEvaluationTracker.WaitForStopped ();
		}

		public ObjectValue GetExpressionValue (EvaluationContext ctx, string exp)
		{
			try {
				ValueReference var = ctx.Evaluator.Evaluate (ctx, exp);
				if (var != null) {
					return var.CreateObjectValue (ctx.Options);
				}
				else
					return ObjectValue.CreateUnknown (exp);
			}
			catch (ImplicitEvaluationDisabledException) {
				return ObjectValue.CreateImplicitNotSupported (ctx.ExpressionValueSource, new ObjectPath (exp), "", ObjectValueFlags.None);
			}
			catch (NotSupportedExpressionException ex) {
				return ObjectValue.CreateNotSupported (ctx.ExpressionValueSource, new ObjectPath (exp), ex.Message, "", ObjectValueFlags.None);
			}
			catch (EvaluatorException ex) {
				return ObjectValue.CreateError (ctx.ExpressionValueSource, new ObjectPath (exp), "", ex.Message, ObjectValueFlags.None);
			}
			catch (Exception ex) {
				ctx.WriteDebuggerError (ex);
				return ObjectValue.CreateUnknown (exp);
			}
		}

		public bool HasMethod (EvaluationContext ctx, object targetType, string methodName)
		{
			BindingFlags flags = BindingFlags.Instance | BindingFlags.Static;
			if (!ctx.Evaluator.CaseSensitive)
				flags |= BindingFlags.IgnoreCase;
			return HasMethod (ctx, targetType, methodName, null, flags);
		}
		
		public bool HasMethod (EvaluationContext ctx, object targetType, string methodName, BindingFlags flags)
		{
			return HasMethod (ctx, targetType, methodName, null, flags);
		}
		
		// argTypes can be null, meaning that it has to return true if there is any method with that name
		// flags will only contain Static or Instance flags
		public virtual bool HasMethod (EvaluationContext ctx, object targetType, string methodName, object[] argTypes, BindingFlags flags)
		{
			return false;
		}
		
		public virtual object RuntimeInvoke (EvaluationContext ctx, object targetType, object target, string methodName, object[] argTypes, object[] argValues)
		{
			return null;
		}
		
		public virtual ValidationResult ValidateExpression (EvaluationContext ctx, string expression)
		{
			return ctx.Evaluator.ValidateExpression (ctx, expression);
		}
	}

	public class TypeDisplayData
	{
		public string ProxyType { get; internal set; }
		public string ValueDisplayString { get; internal set; }
		public string TypeDisplayString { get; internal set; }
		public string NameDisplayString { get; internal set; }
		public bool IsCompilerGenerated { get; internal set; }
		
		public bool IsProxyType {
			get { return ProxyType != null; }
		}

		public static readonly TypeDisplayData Default = new TypeDisplayData (null, null, null, null, false, null);

		public Dictionary<string, DebuggerBrowsableState> MemberData { get; internal set; }
		
		public TypeDisplayData (string proxyType, string valueDisplayString, string typeDisplayString,
			string nameDisplayString, bool isCompilerGenerated, Dictionary<string, DebuggerBrowsableState> memberData)
		{
			ProxyType = proxyType;
			ValueDisplayString = valueDisplayString;
			TypeDisplayString = typeDisplayString;
			NameDisplayString = nameDisplayString;
			IsCompilerGenerated = isCompilerGenerated;
			MemberData = memberData;
		}

		public DebuggerBrowsableState GetMemberBrowsableState (string name)
		{
			if (MemberData == null)
				return DebuggerBrowsableState.Collapsed;

			DebuggerBrowsableState state;
			if (MemberData.TryGetValue (name, out state))
				return state;
			else
				return DebuggerBrowsableState.Collapsed;
		}
	}
	
	public struct EnumMember
	{
		public string Name { get; set; }
		public long Value { get; set; }
	}
}