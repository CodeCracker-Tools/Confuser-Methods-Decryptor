/*
 * Created by SharpDevelop.
 * User: Bogdan
 * Date: 29.12.2012
 * Time: 18:25
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;

namespace Confuser_Methods_Decryptor
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		public string DirectoryName = "";
		void Button1Click(object sender, EventArgs e)
		{
		label3.Text="";
		OpenFileDialog fdlg = new OpenFileDialog();
		fdlg.Title = "Browse for target assembly";
		fdlg.InitialDirectory = @"c:\";
		if (DirectoryName!="") fdlg.InitialDirectory = DirectoryName;
		fdlg.Filter = "Executable files (*.exe,*.dll)|*.exe;*.dll";
		fdlg.FilterIndex = 2;
		fdlg.RestoreDirectory = true;
		if(fdlg.ShowDialog() == DialogResult.OK)
		{
		string FileName = fdlg.FileName;
		textBox1.Text = FileName;
		int lastslash = FileName.LastIndexOf("\\");
		if (lastslash!=-1) DirectoryName = FileName.Remove(lastslash,FileName.Length-lastslash);
        if (DirectoryName.Length==2) DirectoryName=DirectoryName+"\\";
		}
		}
		
		void Button3Click(object sender, EventArgs e)
		{
		label3.Text="";
		OpenFileDialog fdlg = new OpenFileDialog();
		fdlg.Title = "Browse for module";
		fdlg.InitialDirectory = @"c:\";
		if (DirectoryName!="") fdlg.InitialDirectory = DirectoryName;
		fdlg.Filter = "Net module file (*.netmodule)|*.netmodule";
		fdlg.FilterIndex = 2;
		fdlg.RestoreDirectory = true;
		if(fdlg.ShowDialog() == DialogResult.OK)
		{
		string FileName = fdlg.FileName;
		textBox2.Text = FileName;
		int lastslash = FileName.LastIndexOf("\\");
		if (lastslash!=-1) DirectoryName = FileName.Remove(lastslash,FileName.Length-lastslash);
        if (DirectoryName.Length==2) DirectoryName=DirectoryName+"\\";
		}
		}
		
		void TextBox1DragEnter(object sender, DragEventArgs e)
		{
		if (e.Data.GetDataPresent(DataFormats.FileDrop))
        e.Effect = DragDropEffects.Copy;
    	else
    	e.Effect = DragDropEffects.None;
		}
		
		void TextBox2DragEnter(object sender, DragEventArgs e)
		{
		if (e.Data.GetDataPresent(DataFormats.FileDrop))
        e.Effect = DragDropEffects.Copy;
    	else
    	e.Effect = DragDropEffects.None;
		}
		
		void TextBox1DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
		{
		try
        {
        Array a = (Array) e.Data.GetData(DataFormats.FileDrop);
        if(a != null)
        {
        string s = a.GetValue(0).ToString();
        int lastoffsetpoint = s.LastIndexOf(".");
        if (lastoffsetpoint != -1)
        {
        string Extension = s.Substring(lastoffsetpoint);
        Extension=Extension.ToLower();
        if (Extension == ".exe"||Extension == ".dll")
        {
        this.Activate();
        textBox1.Text = s;
        int lastslash = s.LastIndexOf("\\");
        if (lastslash!=-1) DirectoryName = s.Remove(lastslash,s.Length-lastslash);
        if (DirectoryName.Length==2) DirectoryName=DirectoryName+"\\";
        }
        }
        }
        }
        catch
        {

        }
		}
		
		void TextBox2DragDrop(object sender, DragEventArgs e)
		{
		try
        {
        Array a = (Array) e.Data.GetData(DataFormats.FileDrop);
        if(a != null)
        {
        string s = a.GetValue(0).ToString();
        int lastoffsetpoint = s.LastIndexOf(".");
        if (lastoffsetpoint != -1)
        {
        string Extension = s.Substring(lastoffsetpoint);
        Extension=Extension.ToLower();
        if (Extension == ".netmodule")
        {
        this.Activate();
        textBox2.Text = s;
        int lastslash = s.LastIndexOf("\\");
        if (lastslash!=-1) DirectoryName = s.Remove(lastslash,s.Length-lastslash);
        if (DirectoryName.Length==2) DirectoryName=DirectoryName+"\\";
        }
        }
        }
        }
        catch
        {

        }
		}
		
		const byte CorILMethod_TinyFormat = 0x02;
		const byte CorILMethod_FatFormat = 0x03;

[DllImport("kernel32.dll")]
public static extern Boolean AllocConsole();
[DllImport("kernel32.dll")]
public static extern Boolean FreeConsole();
public static Boolean isConsoleon;

		void Button2Click(object sender, EventArgs e)
		{
		string inputfile = textBox1.Text;
		string modulename = textBox2.Text;
		Module mainmodule = null;
		Assembly assembly=null;
		bool isModule = false;
		try
		{

AssemblyName an = AssemblyName.GetAssemblyName(inputfile);
assembly = Assembly.Load(an);
mainmodule = assembly.ManifestModule;
if (File.Exists(modulename))
{
byte[] bytes = File.ReadAllBytes(modulename);
string modname = Path.GetFileName(modulename);
mainmodule = assembly.LoadModule(modname,bytes);
isModule = true;
}
		}
  		catch (Exception loadexception)
		{
  		label3.ForeColor = Color.Red;
  		label3.Text = loadexception.ToString();
  		return;
		}

if (File.Exists(modulename))
{
inputfile = modulename;
}

		MetadataReader mr = new MetadataReader();
		FileStream input = null;
		BinaryReader reader = null;
		MethodBase ep = null;
		bool isConsoleon = false;

if (File.Exists(inputfile))
{
input = new FileStream(inputfile, FileMode.Open, System.IO.FileAccess.Read, FileShare.ReadWrite);
reader = new BinaryReader(input);
if (mr.Intialize(reader))
{
if (mr.netdir.EntryPointToken!=0&&
(mr.netdir.EntryPointToken&0x0FF000000)==0x06000000)
{
int internalEntryPointToken = mr.netdir.EntryPointToken&0x0FFFFFF;
if (internalEntryPointToken>0&&internalEntryPointToken<=mr.TableLengths[6])
{
try
{ // ResolveMethod since on Framework 4.0 System.Reflection.Assembly.EntryPoint()
  // throw new NotImplementedException();
ep = mainmodule.ResolveMethod(mr.netdir.EntryPointToken);
}
catch
{
}
}
if (ep!=null&&mr.inh.ioh.Subsystem==03)
{
try
{

isConsoleon = AllocConsole();
}
catch
{
}
}
// Invoke the entry point (changed to ret) for initing the protector!
if (ep!=null)//
{
// string[] args -> new string[]{null}
object[] parametersers = new object[] { };  // no parameters

try
{
if (ep.GetParameters().Length > 0)
{
if (ep.GetParameters()[0].ParameterType != typeof(string[]))
parametersers = new string[]{null};
else
parametersers = new object[] {null};;
}
}
catch
{
}


try
{
if (ep.IsStatic)
{
ep.Invoke(null, parametersers);
}
else
{
object obj = Activator.CreateInstance(ep.DeclaringType);
ep.Invoke(obj, parametersers);
}
}
catch
{

}
}
if (isConsoleon)
{

try
{
FreeConsole();
}
catch
{
}

}
}
}
reader.Close();
input.Close();
}


  		byte[] filebytes = File.ReadAllBytes(inputfile);
		input = new FileStream(inputfile, FileMode.Open, System.IO.FileAccess.Read, FileShare.ReadWrite);
		reader = new BinaryReader(input);
		mr = new MetadataReader();
		if (mr.Intialize(reader))
		{
long MethodPosition = mr.TablesOffset;
for (int g=0;g<06;g++)
MethodPosition = MethodPosition+mr.tablesize[g].TotalSize*mr.TableLengths[g];

IntPtr hinstance = Marshal.GetHINSTANCE(mainmodule);
for (int i = 0; i < mr.TableLengths[06]; i++)
{
reader.BaseStream.Position=(long)MethodPosition + mr.tablesize[06].TotalSize*i;
int methodRVA = reader.ReadInt32();
if (methodRVA<=0)
continue;

int methodsoffset = mr.Rva2Offset(methodRVA);
if (methodsoffset<=0)
continue;

byte Flag = 0;
if (isModule)
Flag = (byte)(Marshal.ReadByte(hinstance,methodsoffset)&255);
else
Flag = (byte)(Marshal.ReadByte(hinstance,methodRVA)&255);
int codesize=0;

		if (( Flag & 3)==CorILMethod_FatFormat)
		{
		if (isModule)
		codesize = Marshal.ReadInt32(hinstance,methodsoffset+4);
		else
		codesize = Marshal.ReadInt32(hinstance,methodRVA+4);
		codesize = codesize+12;

		}
		else if (( Flag & 3)==CorILMethod_TinyFormat)
		{
		codesize=Flag>>2;
		codesize = codesize+1;
		}
		
		
		try
		{
		byte[]methodbody = new byte[codesize];
		for (int j=0;j<methodbody.Length;j++)
		{
		if (isModule)
		methodbody[j]=Marshal.ReadByte(hinstance,methodsoffset+j);
		else
		methodbody[j]=Marshal.ReadByte(hinstance,methodRVA+j);
		}
		Array.Copy(methodbody,0,filebytes,methodsoffset,methodbody.Length);
		}
		catch (Exception ecx)
		{
		MessageBox.Show(ecx.ToString());

		}

				
		}
		
string directoryname = Path.GetDirectoryName(inputfile);
if (!directoryname.EndsWith("\\")) directoryname=directoryname+"\\";
string newFile = directoryname+Path.GetFileNameWithoutExtension(inputfile)+"_decryptedmethods"+Path.GetExtension(inputfile);
	
		File.WriteAllBytes(newFile,filebytes);
		label3.ForeColor = Color.Blue;
		label3.Text = "Saved on:"+newFile;
		}
		reader.Close();
		input.Close();
		
		}
	}
	
	public class MetadataReader
	{
	
	public enum Types
	{
		//Tables
		Module = 0,
		TypeRef = 1,
		TypeDef = 2,
		FieldPtr = 3, 
		Field = 4,
		MethodPtr = 5,
		Method = 6,
		ParamPtr = 7, 
		Param = 8,
		InterfaceImpl = 9,
		MemberRef = 10,
		Constant = 11, 
		CustomAttribute = 12,
		FieldMarshal = 13,
		Permission = 14,
		ClassLayout = 15, 
		FieldLayout = 16,
		StandAloneSig = 17,
		EventMap = 18,
		EventPtr = 19, 
		Event = 20,
		PropertyMap = 21,
		PropertyPtr = 22,
		Property = 23, 
		MethodSemantics = 24,
		MethodImpl = 25,
		ModuleRef = 26,
		TypeSpec = 27, 
		ImplMap = 28, //lidin book is wrong again here?  It has enclog at 28
		FieldRVA = 29,
		ENCLog = 30,
		ENCMap = 31, 
		Assembly = 32,
		AssemblyProcessor= 33,
		AssemblyOS = 34,
		AssemblyRef = 35, 
		AssemblyRefProcessor = 36,
		AssemblyRefOS = 37,
		File = 38,
		ExportedType = 39, 
		ManifestResource = 40,
		NestedClass = 41,
		TypeTyPar = 42,
		MethodTyPar = 43,

		//Coded Token Types
		TypeDefOrRef = 64,
		HasConstant = 65,
		CustomAttributeType = 66,
		HasSemantic = 67,
		ResolutionScope = 68,
		HasFieldMarshal = 69,
		HasDeclSecurity = 70,
		MemberRefParent = 71,
		MethodDefOrRef = 72,
		MemberForwarded = 73,
		Implementation = 74,
		HasCustomAttribute = 75,

		//Simple
		UInt16 = 97,
		UInt32 = 99,
		String = 101,
		Blob = 102,
		Guid = 103,
		UserString = 112
	}
	
	
unsafe public struct IMAGE_DOS_HEADER
{
     public short e_magic;
     public short e_cblp;
     public short e_cp;
     public short e_crlc;
     public short e_cparhdr;
     public short e_minalloc;
     public short e_maxalloc;
     public short e_ss;
     public short e_sp;
     public short e_csum;
     public short e_ip;
     public short e_cs;
     public short e_lfarlc;
     public short e_ovno;
     fixed short e_res1[4];
     public short e_oeminfo;
     fixed short e_res2[10];
     public int e_lfanew;
}

unsafe public struct IMAGE_NT_HEADERS
{
public int Signature;
public IMAGE_FILE_HEADER ifh;
public IMAGE_OPTIONAL_HEADER ioh;
}
public struct IMAGE_DATA_DIRECTORY
{
public int RVA;
public int Size;
}

public struct IMAGE_FILE_HEADER
{
  public short  Machine;
  public short  NumberOfSections;
  public int TimeDateStamp;
  public int PointerToSymbolTable;
  public int NumberOfSymbols;
  public short  SizeOfOptionalHeader;
  public short  Characteristics;
}

unsafe public struct IMAGE_OPTIONAL_HEADER
{
  public short     Magic;
  public byte      MajorLinkerVersion;
  public byte      MinorLinkerVersion;
  public int       SizeOfCode;
  public int       SizeOfInitializedData;
  public int       SizeOfUninitializedData;
  public int       AddressOfEntryPoint;
  public int       BaseOfCode;
  public int       BaseOfData;
  public int       ImageBase;
  public int       SectionAlignment;
  public int       FileAlignment;
  public short     MajorOperatingSystemVersion;
  public short     MinorOperatingSystemVersion;
  public short     MajorImageVersion;
  public short     MinorImageVersion;
  public short     MajorSubsystemVersion;
  public short     MinorSubsystemVersion;
  public int       Win32VersionValue;
  public int       SizeOfImage;
  public int       SizeOfHeaders;
  public int       CheckSum;
  public short     Subsystem;
  public short     DllCharacteristics;
  public int       SizeOfStackReserve;
  public int       SizeOfStackCommit;
  public int       SizeOfHeapReserve;
  public int       SizeOfHeapCommit;
  public int       LoaderFlags;
  public int       NumberOfRvaAndSizes;
  public IMAGE_DATA_DIRECTORY ExportDirectory;
  public IMAGE_DATA_DIRECTORY ImportDirectory;
  public IMAGE_DATA_DIRECTORY ResourceDirectory;
  public IMAGE_DATA_DIRECTORY ExceptionDirectory;
  public IMAGE_DATA_DIRECTORY SecurityDirectory;
  public IMAGE_DATA_DIRECTORY RelocationDirectory;
  public IMAGE_DATA_DIRECTORY DebugDirectory;
  public IMAGE_DATA_DIRECTORY ArchitectureDirectory;
  public IMAGE_DATA_DIRECTORY Reserved;
  public IMAGE_DATA_DIRECTORY TLSDirectory;
  public IMAGE_DATA_DIRECTORY ConfigurationDirectory;
  public IMAGE_DATA_DIRECTORY BoundImportDirectory;
  public IMAGE_DATA_DIRECTORY ImportAddressTableDirectory;
  public IMAGE_DATA_DIRECTORY DelayImportDirectory;
  public IMAGE_DATA_DIRECTORY MetaDataDirectory;
}

public unsafe struct image_section_header
{
  public fixed byte name[8];
  public int  virtual_size;
  public int  virtual_address;
  public int  size_of_raw_data;
  public int  pointer_to_raw_data;
  public int  pointer_to_relocations;
  public int  pointer_to_linenumbers;
  public short number_of_relocations;
  public short number_of_linenumbers;
  public int  characteristics;
};

public struct NETDirectory
{
public int cb;
public short MajorRuntimeVersion;
public short MinorRuntimeVersion;
public int MetaDataRVA;
public int MetaDataSize;
public int Flags;
public int EntryPointToken;
public int ResourceRVA;
public int ResourceSize;
public int StrongNameSignatureRVA;
public int StrongNameSignatureSize;
public int CodeManagerTableRVA;
public int CodeManagerTableSize;
public int VTableFixupsRVA;
public int VTableFixupsSize;
public int ExportAddressTableJumpsRVA;
public int ExportAddressTableJumpsSize;
public int ManagedNativeHeaderRVA;
public int ManagedNativeHeaderSize;
}

unsafe public struct MetaDataHeader
{
public int Signature;
public short MajorVersion;
public short MinorVersion;
public int Reserved;
public int VerionLenght;
public byte[] VersionString;
public short Flags;
public short NumberOfStreams;
}

unsafe public struct TableHeader
{
public int Reserved_1;
public byte MajorVersion;
public byte MinorVersion;
public byte HeapOffsetSizes;
public byte Reserved_2;
public long MaskValid;
public long MaskSorted;
}

public struct MetaDataStream
{
public int Offset;
public int Size;
public string Name;
}

public struct TableInfo
{
public string Name;
public string[] names;
public Types type;
public Types[] ctypes;
}

public struct RefTableInfo
{
public Types type;
public Types[] reftypes;
public int[] refindex;
}

public struct TableSize
{
public int TotalSize;
public int[] Sizes;
}

public struct Table
{
public long[][] members;
}


		public IMAGE_DOS_HEADER idh;
		public IMAGE_NT_HEADERS inh;
		public image_section_header[] sections;
		public NETDirectory netdir;
		public MetaDataHeader mh;
		public MetaDataStream[] streams;
		public MetaDataStream MetadataRoot;
		public byte[] Strings;
		public byte[] US;
		public byte[] GUID;
		public byte[] Blob;
		public long TablesOffset;
		public TableHeader tableheader;
		public int[] TableLengths;
		public TableInfo[] tablesinfo;
		public RefTableInfo[] reftables;
		public TableSize[] tablesize;
		public int[] codedTokenBits;
		public long BlobOffset;
		public long StringOffset;
		
		public void InitTablesInfo()
		{
tablesinfo = new TableInfo[0x2D];
tablesinfo[0].Name = "Module";
tablesinfo[0].names = new String[] { "Generation", "Name", "Mvid", "EncId", "EncBaseId" };
tablesinfo[0].type = Types.Module;
tablesinfo[0].ctypes = new Types[] { Types.UInt16, Types.String, Types.Guid, Types.Guid, Types.Guid };
tablesinfo[1].Name = "TypeRef";
tablesinfo[1].names = new String[] { "ResolutionScope", "Name", "Namespace" };
tablesinfo[1].type = Types.TypeRef;
tablesinfo[1].ctypes = new Types[] { Types.ResolutionScope, Types.String, Types.String };
tablesinfo[2].Name = "TypeDef";
tablesinfo[2].names = new String[] { "Flags", "Name", "Namespace", "Extends", "FieldList", "MethodList" };
tablesinfo[2].type = Types.TypeDef;
tablesinfo[2].ctypes = new Types[] { Types.UInt32, Types.String, Types.String, Types.TypeDefOrRef, Types.Field, Types.Method };
tablesinfo[3].Name = "FieldPtr";
tablesinfo[3].names = new String[] { "Field" };
tablesinfo[3].type = Types.FieldPtr;
tablesinfo[3].ctypes = new Types[] { Types.Field };
tablesinfo[4].Name = "Field";
tablesinfo[4].names = new String[] { "Flags", "Name", "Signature" };
tablesinfo[4].type = Types.Field;
tablesinfo[4].ctypes = new Types[] { Types.UInt16, Types.String, Types.Blob };
tablesinfo[5].Name = "MethodPtr";
tablesinfo[5].names = new String[] { "Method"};
tablesinfo[5].type = Types.MethodPtr;
tablesinfo[5].ctypes = new Types[] { Types.Method };
tablesinfo[6].Name = "Method";
tablesinfo[6].names = new String[] { "RVA", "ImplFlags", "Flags", "Name", "Signature", "ParamList" };
tablesinfo[6].type = Types.Method;
tablesinfo[6].ctypes = new Types[] { Types.UInt32, Types.UInt16, Types.UInt16, Types.String, Types.Blob, Types.Param };
tablesinfo[7].Name = "ParamPtr";
tablesinfo[7].names = new String[] { "Param" };
tablesinfo[7].type = Types.ParamPtr;
tablesinfo[7].ctypes = new Types[] { Types.Param };
tablesinfo[8].Name = "Param";
tablesinfo[8].names = new String[] { "Flags", "Sequence", "Name" };
tablesinfo[8].type = Types.Param;
tablesinfo[8].ctypes = new Types[] { Types.UInt16, Types.UInt16, Types.String };
tablesinfo[9].Name = "InterfaceImpl";
tablesinfo[9].names = new String[] { "Class", "Interface" };
tablesinfo[9].type = Types.InterfaceImpl;
tablesinfo[9].ctypes = new Types[] { Types.TypeDef, Types.TypeDefOrRef };
tablesinfo[0x0A].Name = "MemberRef";
tablesinfo[0x0A].names = new String[] { "Class", "Name", "Signature" };
tablesinfo[0x0A].type = Types.MemberRef;
tablesinfo[0x0A].ctypes = new Types[] { Types.MemberRefParent, Types.String, Types.Blob };
tablesinfo[0x0B].Name = "Constant";
tablesinfo[0x0B].names = new String[] { "Type", "Parent", "Value" };
tablesinfo[0x0B].type = Types.Constant;
tablesinfo[0x0B].ctypes = new Types[] { Types.UInt16, Types.HasConstant, Types.Blob };
tablesinfo[0x0C].Name = "CustomAttribute";
tablesinfo[0x0C].names = new String[] { "Type", "Parent", "Value" };
tablesinfo[0x0C].type = Types.CustomAttribute;
tablesinfo[0x0C].ctypes = new Types[] { Types.HasCustomAttribute, Types.CustomAttributeType, Types.Blob };
tablesinfo[0x0D].Name = "FieldMarshal";
tablesinfo[0x0D].names = new String[] { "Parent", "Native" };
tablesinfo[0x0D].type = Types.FieldMarshal;
tablesinfo[0x0D].ctypes = new Types[] { Types.HasFieldMarshal, Types.Blob };
tablesinfo[0x0E].Name = "Permission";
tablesinfo[0x0E].names = new String[] { "Action", "Parent", "PermissionSet" };
tablesinfo[0x0E].type = Types.Permission;
tablesinfo[0x0E].ctypes = new Types[] { Types.UInt16, Types.HasDeclSecurity, Types.Blob };
tablesinfo[0x0F].Name = "ClassLayout";
tablesinfo[0x0F].names = new String[] { "PackingSize", "ClassSize", "Parent" };
tablesinfo[0x0F].type = Types.ClassLayout;
tablesinfo[0x0F].ctypes = new Types[] { Types.UInt16, Types.UInt32, Types.TypeDef };
tablesinfo[0x10].Name = "FieldLayout";
tablesinfo[0x10].names = new String[] { "Offset", "Field" };
tablesinfo[0x10].type = Types.FieldLayout;
tablesinfo[0x10].ctypes = new Types[] { Types.UInt32, Types.Field };
tablesinfo[0x11].Name = "StandAloneSig";
tablesinfo[0x11].names = new String[] { "Signature" };
tablesinfo[0x11].type = Types.StandAloneSig;
tablesinfo[0x11].ctypes = new Types[] { Types.Blob };
tablesinfo[0x12].Name = "EventMap";
tablesinfo[0x12].names = new String[] { "Parent", "EventList" };
tablesinfo[0x12].type = Types.EventMap;
tablesinfo[0x12].ctypes = new Types[] { Types.TypeDef, Types.Event };
tablesinfo[0x13].Name = "EventPtr";
tablesinfo[0x13].names = new String[] { "Event" };
tablesinfo[0x13].type = Types.EventPtr;
tablesinfo[0x13].ctypes = new Types[] { Types.Event };
tablesinfo[0x14].Name = "Event";
tablesinfo[0x14].names = new String[] { "EventFlags", "Name", "EventType" };
tablesinfo[0x14].type = Types.Event;
tablesinfo[0x14].ctypes = new Types[] { Types.UInt16, Types.String, Types.TypeDefOrRef };
tablesinfo[0x15].Name = "PropertyMap";
tablesinfo[0x15].names = new String[] { "Parent", "PropertyList" };
tablesinfo[0x15].type = Types.PropertyMap;
tablesinfo[0x15].ctypes = new Types[] { Types.TypeDef, Types.Property };
tablesinfo[0x16].Name = "PropertyPtr";
tablesinfo[0x16].names = new String[] { "Property" };
tablesinfo[0x16].type = Types.PropertyPtr;
tablesinfo[0x16].ctypes = new Types[] { Types.Property };
tablesinfo[0x17].Name = "Property";
tablesinfo[0x17].names = new String[] { "PropFlags", "Name", "Type" };
tablesinfo[0x17].type = Types.Property;
tablesinfo[0x17].ctypes = new Types[] { Types.UInt16, Types.String, Types.Blob };
tablesinfo[0x18].Name = "MethodSemantics";
tablesinfo[0x18].names = new String[] { "Semantic", "Method", "Association" };
tablesinfo[0x18].type = Types.MethodSemantics;
tablesinfo[0x18].ctypes = new Types[] { Types.UInt16, Types.Method, Types.HasSemantic };
tablesinfo[0x19].Name = "MethodImpl";
tablesinfo[0x19].names = new String[] { "Class", "MethodBody", "MethodDeclaration" };
tablesinfo[0x19].type = Types.MethodImpl;
tablesinfo[0x19].ctypes = new Types[] { Types.TypeDef, Types.MethodDefOrRef, Types.MethodDefOrRef };
tablesinfo[0x1A].Name = "ModuleRef";
tablesinfo[0x1A].names = new String[] { "Name" };
tablesinfo[0x1A].type = Types.ModuleRef;
tablesinfo[0x1A].ctypes = new Types[] { Types.String };
tablesinfo[0x1B].Name = "TypeSpec";
tablesinfo[0x1B].names = new String[] { "Signature" };
tablesinfo[0x1B].type = Types.TypeSpec;
tablesinfo[0x1B].ctypes = new Types[] { Types.Blob };
tablesinfo[0x1C].Name = "ImplMap";
tablesinfo[0x1C].names = new String[] { "MappingFlags", "MemberForwarded", "ImportName", "ImportScope" };
tablesinfo[0x1C].type = Types.ImplMap;
tablesinfo[0x1C].ctypes = new Types[] { Types.UInt16, Types.MemberForwarded, Types.String, Types.ModuleRef };
tablesinfo[0x1D].Name = "FieldRVA";
tablesinfo[0x1D].names = new String[] { "RVA", "Field" };
tablesinfo[0x1D].type = Types.FieldRVA;
tablesinfo[0x1D].ctypes = new Types[] { Types.UInt32, Types.Field };
tablesinfo[0x1E].Name = "ENCLog";
tablesinfo[0x1E].names = new String[] { "Token", "FuncCode" };
tablesinfo[0x1E].type = Types.ENCLog;
tablesinfo[0x1E].ctypes = new Types[] { Types.UInt32, Types.UInt32 };
tablesinfo[0x1F].Name = "ENCMap";
tablesinfo[0x1F].names = new String[] { "Token" };
tablesinfo[0x1F].type = Types.ENCMap;
tablesinfo[0x1F].ctypes = new Types[] { Types.UInt32 };
tablesinfo[0x20].Name = "Assembly";
tablesinfo[0x20].names = new String[] { "HashAlgId", "MajorVersion", "MinorVersion", "BuildNumber", "RevisionNumber", "Flags", "PublicKey", "Name", "Locale" };
tablesinfo[0x20].type = Types.Assembly;
tablesinfo[0x20].ctypes = new Types[] { Types.UInt32, Types.UInt16, Types.UInt16, Types.UInt16, Types.UInt16, Types.UInt32, Types.Blob, Types.String, Types.String };
tablesinfo[0x21].Name = "AssemblyProcessor";
tablesinfo[0x21].names = new String[] { "Processor" };
tablesinfo[0x21].type = Types.AssemblyProcessor;
tablesinfo[0x21].ctypes = new Types[] { Types.UInt32 };
tablesinfo[0x22].Name = "AssemblyOS";
tablesinfo[0x22].names = new String[] { "OSPlatformId", "OSMajorVersion", "OSMinorVersion" };
tablesinfo[0x22].type = Types.AssemblyOS;
tablesinfo[0x22].ctypes = new Types[] { Types.UInt32, Types.UInt32, Types.UInt32 };
tablesinfo[0x23].Name = "AssemblyRef";
tablesinfo[0x23].names = new String[] { "MajorVersion", "MinorVersion", "BuildNumber", "RevisionNumber", "Flags", "PublicKeyOrToken", "Name", "Locale", "HashValue" };
tablesinfo[0x23].type = Types.AssemblyRef;
tablesinfo[0x23].ctypes = new Types[] { Types.UInt16, Types.UInt16, Types.UInt16, Types.UInt16, Types.UInt32, Types.Blob, Types.String, Types.String, Types.Blob };
tablesinfo[0x24].Name = "AssemblyRefProcessor";
tablesinfo[0x24].names = new String[] { "Processor", "AssemblyRef" };
tablesinfo[0x24].type = Types.AssemblyRefProcessor;
tablesinfo[0x24].ctypes = new Types[] { Types.UInt32, Types.AssemblyRef };
tablesinfo[0x25].Name = "AssemblyRefOS";
tablesinfo[0x25].names = new String[] { "OSPlatformId", "OSMajorVersion", "OSMinorVersion", "AssemblyRef" };
tablesinfo[0x25].type = Types.AssemblyRefOS;
tablesinfo[0x25].ctypes = new Types[] { Types.UInt32, Types.UInt32, Types.UInt32, Types.AssemblyRef };
tablesinfo[0x26].Name = "File";
tablesinfo[0x26].names = new String[] { "Flags", "Name", "HashValue" };
tablesinfo[0x26].type = Types.File;
tablesinfo[0x26].ctypes = new Types[] { Types.UInt32, Types.String, Types.Blob };
tablesinfo[0x27].Name = "ExportedType";
tablesinfo[0x27].names = new String[] { "Flags", "TypeDefId", "TypeName", "TypeNamespace", "TypeImplementation" };
tablesinfo[0x27].type = Types.ExportedType;
tablesinfo[0x27].ctypes = new Types[] { Types.UInt32, Types.UInt32, Types.String, Types.String, Types.Implementation };
tablesinfo[0x28].Name = "ManifestResource";
tablesinfo[0x28].names = new String[] { "Offset", "Flags", "Name", "Implementation" };
tablesinfo[0x28].type = Types.ManifestResource;
tablesinfo[0x28].ctypes = new Types[] { Types.UInt32, Types.UInt32, Types.String, Types.Implementation };
tablesinfo[0x29].Name = "NestedClass";
tablesinfo[0x29].names = new String[] { "NestedClass", "EnclosingClass"};
tablesinfo[0x29].type = Types.NestedClass;
tablesinfo[0x29].ctypes = new Types[] { Types.TypeDef, Types.TypeDef };
//unused TyPar tables taken from Roeder's reflector... are these documented anywhere?  Since they are always empty, does it matter
tablesinfo[0x2A].Name = "TypeTyPar";
tablesinfo[0x2A].names = new String[] { "Number", "Class", "Bound", "Name" };
tablesinfo[0x2A].type = Types.TypeTyPar;
tablesinfo[0x2A].ctypes = new Types[] { Types.UInt16, Types.TypeDef, Types.TypeDefOrRef, Types.String };
tablesinfo[0x2B].Name = "MethodTyPar";
tablesinfo[0x2B].names = new String[] { "Number", "Method", "Bound", "Name" };
tablesinfo[0x2B].type = Types.MethodTyPar;
tablesinfo[0x2B].ctypes = new Types[] { Types.UInt16, Types.Method, Types.TypeDefOrRef, Types.String };
tablesinfo[0x2C].Name = "MethodTyPar";
tablesinfo[0x2C].names = new String[] { "Number", "Method", "Bound", "Name" };
tablesinfo[0x2C].type = Types.MethodTyPar;
tablesinfo[0x2C].ctypes = new Types[] { Types.UInt16, Types.Method, Types.TypeDefOrRef, Types.String };

//number of bits in coded token tag for a coded token that refers to n tables.
//values 5-17 are not used :I		
codedTokenBits = new int[] { 0, 1, 1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5 };
reftables = new RefTableInfo[12];
reftables[0].type = Types.TypeDefOrRef;
reftables[0].reftypes = new Types[] { Types.TypeDef, Types.TypeRef, Types.TypeSpec};
reftables[0].refindex = new int[] { 1, 2, 0x1B};
reftables[1].type = Types.HasConstant;
reftables[1].reftypes = new Types[] { Types.Field, Types.Param, Types.Property };
reftables[1].refindex = new int[] { 04, 08, 0x17};
reftables[2].type = Types.CustomAttributeType;
reftables[2].reftypes = new Types[] { Types.TypeRef, Types.TypeDef, Types.Method, Types.MemberRef, Types.UserString };
reftables[2].refindex = new int[] { 01, 02, 06, 0x0A,01};  //????????
reftables[3].type = Types.HasSemantic;
reftables[3].reftypes = new Types[] { Types.Event, Types.Property };
reftables[3].refindex = new int[] { 0x14, 0x17};
reftables[4].type = Types.ResolutionScope;
reftables[4].reftypes = new Types[] { Types.Module, Types.ModuleRef, Types.AssemblyRef, Types.TypeRef };
reftables[4].refindex = new int[] { 00, 0x1A,0x23, 0x01};
reftables[5].type = Types.HasFieldMarshal;
reftables[5].reftypes = new Types[] { Types.Field, Types.Param };
reftables[5].refindex = new int[] { 04, 08};
reftables[6].type = Types.HasDeclSecurity;
reftables[6].reftypes = new Types[] { Types.TypeDef, Types.Method, Types.Assembly };
reftables[6].refindex = new int[] { 02, 06, 0x20};
reftables[7].type = Types.MemberRefParent;
reftables[7].reftypes = new Types[] { Types.TypeDef, Types.TypeRef, Types.ModuleRef, Types.Method, Types.TypeSpec };
reftables[7].refindex = new int[] { 02,01,0x1A,06,0x1B };
reftables[8].type = Types.MethodDefOrRef;
reftables[8].reftypes = new Types[] { Types.Method, Types.MemberRef };
reftables[8].refindex = new int[] { 06,0x0A };
reftables[9].type = Types.MemberForwarded;
reftables[9].reftypes = new Types[] { Types.Field, Types.Method };
reftables[9].refindex = new int[] { 04,06 };
reftables[10].type = Types.Implementation;
reftables[10].reftypes = new Types[] { Types.File, Types.AssemblyRef, Types.ExportedType };
reftables[10].refindex = new int[] { 0x26, 0x23,0x27 };
reftables[11].type = Types.HasCustomAttribute;
reftables[11].reftypes = new Types[] { Types.Method, Types.Field, Types.TypeRef, Types.TypeDef, Types.Param, Types.InterfaceImpl, Types.MemberRef, Types.Module, Types.Permission, Types.Property, Types.Event, Types.StandAloneSig, Types.ModuleRef, Types.TypeSpec, Types.Assembly, Types.AssemblyRef, Types.File, Types.ExportedType, Types.ManifestResource };
reftables[11].refindex = new int[] { 06, 04, 01,02,08,09,0x0A,00,0x0E,0x17,0x14,0x11,0x1A,0x1B,0x20,0x23,0x26,0x27,0x28};
	}

		public int Rva2Offset(int rva)
		{
			for (int i = 0; i < sections.Length; i++)
			{
					if ((sections[i].virtual_address <= rva) && (sections[i].virtual_address + sections[i].size_of_raw_data >= rva))
					return (sections[i].pointer_to_raw_data + (rva - sections[i].virtual_address));
			}
		
			return 0;
		}
		
		public int Offset2Rva(int uOffset)
		{
			for (int i = 0; i < sections.Length; i++)
			{
				if ((sections[i].pointer_to_raw_data <= uOffset) && ((sections[i].pointer_to_raw_data + sections[i].size_of_raw_data) >= uOffset))
				return (sections[i].virtual_address + (uOffset - sections[i].pointer_to_raw_data));
			}
		
			return 0;
		}
		
		public int GetTypeSize(Types trans)
		{
		if (trans == Types.UInt16) return 2;
		if (trans == Types.UInt32) return 4;
		
		// Heap
		if (trans == Types.String) return GetStringIndexSize();
		if (trans == Types.Guid) return GetGuidIndexSize();
		if (trans == Types.Blob) return GetBlobIndexSize();

		// Rid
		if ((int)trans < 64)
		{
		if (TableLengths[(int)trans] > 65535) return 4;
		else return 2;
		}
		// Coded token (may need to be uncompressed from 2-byte form)
		else if ((int)trans < 97)
		{
		int index = (int)trans-64;
		int codedtokenbits = codedTokenBits[reftables[index].refindex.Length];
		int maxsizekeeped = 65535;
		maxsizekeeped = maxsizekeeped >> codedtokenbits;
		for (int i=0;i<reftables[index].refindex.Length;i++)
		{
		if (TableLengths[reftables[index].refindex[i]] > maxsizekeeped) return 4;
		}
		return 2;
		
		}
		
		return 0;
		}

		public int GetStringIndexSize()
		{
		return ((tableheader.HeapOffsetSizes & 0x01) != 0) ? 4 : 2;
		}
				
		public int GetGuidIndexSize()
		{
		return ((tableheader.HeapOffsetSizes & 0x02) != 0) ? 4 : 2;
		}
		
		public int GetBlobIndexSize()
		{
		return ((tableheader.HeapOffsetSizes & 0x04) != 0) ? 4 : 2;
		}
		
		public unsafe bool Intialize(BinaryReader reader)
		{
		reader.BaseStream.Position=0;
		byte[] buffer;
		IntPtr pointer;
		long NewPos;
		
		try
		{
		buffer = reader.ReadBytes(sizeof(IMAGE_DOS_HEADER));
		
		fixed (byte* p = buffer)
		{
		pointer = (IntPtr)p;
		}
		idh = (IMAGE_DOS_HEADER)Marshal.PtrToStructure(pointer, typeof(IMAGE_DOS_HEADER));
		if (idh.e_magic!=0x5A4D) return false;
		reader.BaseStream.Position=idh.e_lfanew;
		buffer = reader.ReadBytes(sizeof(IMAGE_NT_HEADERS));

		fixed (byte* p = buffer)
		{
		pointer = (IntPtr)p;
		}
		inh = (IMAGE_NT_HEADERS)Marshal.PtrToStructure(pointer, typeof(IMAGE_NT_HEADERS));
		if (inh.Signature!=0x4550) return false;
		
		reader.BaseStream.Position=idh.e_lfanew+4+sizeof(IMAGE_FILE_HEADER)+inh.ifh.SizeOfOptionalHeader;
		sections=new image_section_header[inh.ifh.NumberOfSections];
		buffer = reader.ReadBytes(sizeof(image_section_header)*inh.ifh.NumberOfSections);
		fixed (byte* p = buffer)
		{
		pointer = (IntPtr)p;
		}
		
		for (int i = 0; i < sections.Length; i++)
		{
		sections[i] = (image_section_header)Marshal.PtrToStructure(pointer, typeof(image_section_header));
		pointer = (IntPtr)(pointer.ToInt32() + Marshal.SizeOf(typeof(image_section_header)));
		}
		
		
		// NET Directory
		if (inh.ioh.MetaDataDirectory.RVA==0) return false;
		NewPos = (long)Rva2Offset(inh.ioh.MetaDataDirectory.RVA);
		if (NewPos==0) return false;
		reader.BaseStream.Position=NewPos;
		buffer = reader.ReadBytes(sizeof(NETDirectory));

		fixed (byte* p = buffer)
		{
		pointer = (IntPtr)p;
		}
		// After .NET Directory comes body of methods!
		netdir = (NETDirectory)Marshal.PtrToStructure(pointer, typeof(NETDirectory));
		
		reader.BaseStream.Position=(long)Rva2Offset(netdir.MetaDataRVA);
		mh = new MetadataReader.MetaDataHeader();
		mh.Signature = reader.ReadInt32();
		mh.MajorVersion = reader.ReadInt16();
		mh.MinorVersion = reader.ReadInt16();
		mh.Reserved = reader.ReadInt32();
		mh.VerionLenght = reader.ReadInt32();
		mh.VersionString = reader.ReadBytes(mh.VerionLenght);
		mh.Flags = reader.ReadInt16();
		mh.NumberOfStreams = reader.ReadInt16();
		
		streams = new MetaDataStream[mh.NumberOfStreams];
			
			for(int i=0; i< mh.NumberOfStreams;++i)
			{
			streams[i].Offset=reader.ReadInt32();
			streams[i].Size=reader.ReadInt32();
			char[] chars = new char[32];
			int index = 0;
			byte character = 0;
			while ((character = reader.ReadByte()) != 0) 
				chars[index++] = (char) character;
	
			index++;
			int padding = ((index % 4) != 0) ? (4 - (index % 4)) : 0;
			reader.ReadBytes(padding);
	
			streams[i].Name = new String(chars).Trim(new Char[] {'\0'});
			
			if (streams[i].Name == "#~"||streams[i].Name == "#-")
			{
			MetadataRoot.Name=streams[i].Name;
			MetadataRoot.Offset=streams[i].Offset;
			MetadataRoot.Size=streams[i].Size;
			}

			if (streams[i].Name == "#Strings") 
			{
			long savepoz = reader.BaseStream.Position;
			reader.BaseStream.Position = (long)(Rva2Offset(netdir.MetaDataRVA)+streams[i].Offset);
			StringOffset=reader.BaseStream.Position;
			Strings = reader.ReadBytes(streams[i].Size);
			reader.BaseStream.Position = savepoz;
			}
			
			if (streams[i].Name == "#US")
			{
			long savepoz = reader.BaseStream.Position;
			reader.BaseStream.Position = (long)(Rva2Offset(netdir.MetaDataRVA)+streams[i].Offset);
			US = reader.ReadBytes(streams[i].Size);
			reader.BaseStream.Position = savepoz;
			}
			
			if (streams[i].Name == "#Blob")
			{
			long savepoz = reader.BaseStream.Position;
			reader.BaseStream.Position = (long)(Rva2Offset(netdir.MetaDataRVA)+streams[i].Offset);
			BlobOffset=reader.BaseStream.Position;
			Blob = reader.ReadBytes(streams[i].Size);
			reader.BaseStream.Position = savepoz;
			}
			
			if (streams[i].Name == "#GUID")
			{
			long savepoz = reader.BaseStream.Position;
			reader.BaseStream.Position = (long)(Rva2Offset(netdir.MetaDataRVA)+streams[i].Offset);
			GUID = reader.ReadBytes(streams[i].Size);
			reader.BaseStream.Position = savepoz;
			}
			
			}
			
			reader.BaseStream.Position=(long)(Rva2Offset(netdir.MetaDataRVA)+MetadataRoot.Offset);
			buffer = reader.ReadBytes(sizeof(TableHeader));
			fixed (byte* p = buffer)
			{
			pointer = (IntPtr)p;
			}
			tableheader = (TableHeader)Marshal.PtrToStructure(pointer, typeof(TableHeader));
			TableLengths = new int[64];
			

			//read as many uints as there are bits set in maskvalid
			for (int i = 0; i < 64; i++)
			{
				int count = (((tableheader.MaskValid >> i) & 1) == 0) ? 0 : reader.ReadInt32();
				TableLengths[i] = count;
			}
			
TablesOffset = reader.BaseStream.Position;
InitTablesInfo();
// Get Table sizes and all Tables
tablesize = new TableSize[0x2D];

for (int i=0;i<tablesize.Length;i++)
{
tablesize[i].Sizes=new int[tablesinfo[i].ctypes.Length];
tablesize[i].TotalSize=0;
 for (int j=0;j<tablesinfo[i].ctypes.Length;j++)
 {
 tablesize[i].Sizes[j]=GetTypeSize(tablesinfo[i].ctypes[j]);
 tablesize[i].TotalSize=tablesize[i].TotalSize+tablesize[i].Sizes[j];
 }

}


 }
 catch
 {
 return false;
 }
return true;

	}
	}
}
