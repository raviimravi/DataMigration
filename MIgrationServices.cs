using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using EasyParse;
using Newtonsoft.Json;
using System.Security.Permissions;
using Microsoft.SharePoint.Client;
using System.Security;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CSOM.UserProfile;
using System.IdentityModel.Tokens;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using LinqToExcel;
using OfficeOpenXml;
using System.Configuration;
using Mapster;
using AutoMapper;
using Microsoft.SharePoint;
namespace CSOM.Test
{
    class UserId
    {
        public int id { get; set; }
    }
    class JsonData
    {
        public List<Part> PartColl { get; set; }
        public Links Hyperlink { get; set; }
        public List<Approvals> Approvals { get; set; }
        public List<Person2> Distributors { get; set; }
    }
    class DeviationMigration
    {
        public static void ConvertXmlToObject()
        {
            List<DeviationFields> DeviationForms = new List<DeviationFields>();

            string path = "C:/Users/raviteja.d/Documents/ITRON/Deviation Data";
            DirectoryInfo DataDirectory = new DirectoryInfo(path);
            FileInfo[] Files = DataDirectory.GetFiles("*.xml");
            foreach (FileInfo file in Files)
            {
                DeviationFields DeviationForm = new DeviationFields() { Approvals = new List<Approvals>(), Parts = new Parts() { Part = new List<Part>() }, FileAttachements = new FileAttachements() { Attachements = new List<Attachements>() }, Description = new Description(), Reason = new Reason(), HyperlinkGroup = new HyperlinkGroup(), ShipmentStatus = new ShipmentStatus(), MaterialSubstitution = new MaterialSubstitution(), Distribution = new Distribution() };
                Console.WriteLine(file.Name);
                string ItemTitle = file.Name.Split('.').First();
                DeviationForm.Title = ItemTitle;
                List<Xml> testlist = new List<Xml>();
                XmlTextReader reader = new XmlTextReader(path + "/" + file.Name);
                string currentpropertyname = "";
                while (reader.Read())
                {
                    Xml xmlobj = new Xml() { NodeType = reader.NodeType.ToString(), NodeName = reader.Name.ToString() };
                    testlist.Add(xmlobj);
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "my:Parts":
                                DeviationForm = CheckPartsCollection(DeviationForm, reader);
                                break;
                            case "my:FileAttachements":
                                DeviationForm = CheckAttachmentsCollection(DeviationForm, reader);
                                break;
                            case "my:HyperlinkGroup":
                                DeviationForm = HyperlinkField(DeviationForm, reader);
                                break;
                            case "my:ShipmentStatus":
                                DeviationForm = ShipmentStatusFields(DeviationForm, reader);
                                break;
                            case "my:MaterialSubstitution":
                                DeviationForm = MaterialSubstitutionFields(DeviationForm, reader);
                                break;
                            case "my:Distribution":
                                DeviationForm = DistributionFields(DeviationForm, reader);
                                break;
                            case "my:Approvals":
                                DeviationForm = ApprovalsFields(DeviationForm, reader);
                                break;
                            case "my:Description":
                                DeviationForm = DescriptionFields(DeviationForm, reader);
                                break;
                            case "my:Reason":
                                DeviationForm = ReasonFields(DeviationForm, reader);
                                break;
                            default:
                                currentpropertyname = reader.Name.Split(':')[1];
                                break;

                        }
                    }
                    if (reader.NodeType == XmlNodeType.Text && DeviationForm.GetType().GetProperty(currentpropertyname) != null && Convert.ToInt32(DeviationForm.GetType().GetProperty(currentpropertyname).Name.Length) > 0)
                    {
                        DeviationForm.GetType().GetProperty(currentpropertyname).SetValue(DeviationForm, reader.Value);
                    }
                }
                DeviationForms.Add(DeviationForm);
            }

            //GetOnpremData();
            Console.WriteLine("Press 1 to view Deviation Form Data\nPress 2 to Add Form to SP List\n press any other key to exit");
            string choice = Console.ReadLine();
            if (choice == "1")
            {
                DeviationFields SelectedForm = new DeviationFields();
                Console.WriteLine("Enter Form ID to view form");
                string ID = Console.ReadLine();
                SelectedForm = DeviationForms.Find(element => element.Title == ID);
                PrintData(SelectedForm);

            }
            else if (choice == "2")
            {
                DeviationFields SelectedForm = new DeviationFields();
                Console.WriteLine("Enter Form ID to add");
                string ID = Console.ReadLine();
                SelectedForm = DeviationForms.Find(element => element.Title == ID);

                CreateItemInList("Deviations", SelectedForm);

            }
        }
        /*public static void WriteDataToTextFile(DeviationFields DeviationForm)
        {
            string filePath = "C:/Users/raviteja.d/Documents/ITRON/text.txt";
            TextWriter writer = null;
            var contentsToWriteToFile = DeviationForm.Description.html.p;
            using (writer = new StreamWriter(filePath, false))
            {
                writer.Write(contentsToWriteToFile);
            }
        }*/
        public static void PrintAttachmentsData(DeviationFields DeviationForm)
        {
            foreach (Attachements attachment in DeviationForm.FileAttachements.Attachements)
            {
                Console.WriteLine(attachment.Attachment);
            }
        }
        public static void PrintData(DeviationFields DeviationForm)
        {
            Console.WriteLine("RequestSite:{0}", DeviationForm.RequestSite);
            Console.WriteLine("RequestNumber:{0}", DeviationForm.RequestNumber);
            Console.WriteLine("Date:{0}", DeviationForm.Date);
            Console.WriteLine("Originator:{0}", DeviationForm.Originator);
            Console.WriteLine("DeviationType:{0}", DeviationForm.DeviationType);
            Console.WriteLine("ExpirationDate:{0}", DeviationForm.ExpirationDate);
            Console.WriteLine("OrganizationId:{0}", DeviationForm.OrganizationId);
            foreach (Part part in DeviationForm.Parts.Part)
                Console.WriteLine("Parts:\n\tPart\n\t\tPartNumber:{0}\n\t\tPartDescription:{1}\n\t\tPartRevision:{2}\n\t\tQuantity:{3}\n\tPart\n\tPartsList:{4}", part.PartNumber, part.PartDescription, part.PartRevision, part.Quantity, DeviationForm.Parts.PartsList);
            Console.WriteLine("Description:{0}", DeviationForm.Description.html.p);
            Console.WriteLine("Reason:{0}", DeviationForm.Reason.html.p.TrimStart('?'));
            Console.WriteLine("HyperlinkGroup:{0}", DeviationForm.HyperlinkGroup.HyperLinks.Links);
            Console.WriteLine("ShipmentStatus:\n\tShipStatus:{0}\n\tShipComments:{1}", DeviationForm.ShipmentStatus.ShipStatus, DeviationForm.ShipmentStatus.ShipComments);
            Console.WriteLine("MaterialSubstitution:\n\tSubstitutionSupplier:{0}\n\tSubstitutedPartNumber:{1}\n\tSubstitutedPartDescription:{2}\n\tSubstitutedPartCost:{3}\n\tReplacedPartCost:{4}\n\tCARRequired:{5}", DeviationForm.MaterialSubstitution.SubstitutionSupplier, DeviationForm.MaterialSubstitution.SubstitutedPartNumber, DeviationForm.MaterialSubstitution.SubstitutedPartDescription, DeviationForm.MaterialSubstitution.SubstitutedPartCost, DeviationForm.MaterialSubstitution.ReplacedPartCost, DeviationForm.MaterialSubstitution.CARRequired);
            foreach (Approvals approval in DeviationForm.Approvals)
                Console.WriteLine("Approvals\n\tApproval\n\t\tFunction:{0}\n\t\tName\n\t\t\tPerson\n\t\t\t\tDisplayName:{1}\n\t\t\t\tAccountId:{2}\n\t\t\t\tAccountType:{3}", approval.Approval.Function, approval.Approval.Name.Person.DisplayName, approval.Approval.Name.Person.AccountId, approval.Approval.Name.Person.AccountType);
            foreach (Person2 person in DeviationForm.Distribution.Person)
                Console.WriteLine("Distribution:\n\tPerson\n\t\tDisplayName:{0}\n\t\tAccountId:{1}\n\t\tAccountType:{2}", person.DisplayName, person.AccountId, person.AccountType);
            Console.WriteLine("DeviationApprovers:{0}", DeviationForm.DeviationApprovers);
            Console.WriteLine("DeviationDistributionGroup:{0}", DeviationForm.DeviationDistributionGroup);
            Console.WriteLine("Press 1 to view attachments field");
            var choice = Console.ReadLine();
            if (choice == "1")
                PrintAttachmentsData(DeviationForm);
        }
        public static DeviationFields ApprovalsFields(DeviationFields Devform, XmlTextReader reader)
        {
            string currentNodeTypeText = null;
            string parentNodeName = reader.Name.ToString();
            string childNodeText = null;
            Approvals ApprovalFields = new Approvals() { Approval = new Approval { Name = new Name() { Person = new Person() } } };

            while (reader.Read())
            {
                if (!(reader.NodeType == XmlNodeType.EndElement && reader.Name.ToString() == "my:Approvals"))
                {
                    if (reader.Name == "my:Function" || reader.Name == "pc:Person")
                        currentNodeTypeText = reader.Name;
                    else
                    {
                        if (reader.NodeType == XmlNodeType.Element && currentNodeTypeText == "pc:Person")
                        {
                            childNodeText = reader.Name.Split(':')[1];
                        }
                        if (reader.NodeType == XmlNodeType.Text && currentNodeTypeText == "pc:Person" && ApprovalFields.Approval.Name.Person.GetType().GetProperty(childNodeText) != null)
                            ApprovalFields.Approval.Name.Person.GetType().GetProperty(childNodeText).SetValue(ApprovalFields.Approval.Name.Person, reader.Value);

                        else if (reader.NodeType == XmlNodeType.Text && currentNodeTypeText == "my:Function" && ApprovalFields.Approval.GetType().GetProperty(currentNodeTypeText.Split(':')[1]) != null)
                            ApprovalFields.Approval.GetType().GetProperty(currentNodeTypeText.Split(':')[1]).SetValue(ApprovalFields.Approval, reader.Value);

                    }
                    if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "my:Approval")
                        Devform.Approvals.Add(ApprovalFields);
                }
                else
                {
                    return Devform;
                }
            }
            return null;
        }
        public static DeviationFields CheckPartsCollection(DeviationFields Devform, XmlTextReader reader)
        {
            string currentNodeTypeText = null;
            string parentNodeName = reader.Name.ToString();
            string childNodeText = null;

            Parts partNumberObj = new Parts() { Part = new List<Part>(), PartsList = "" };
            Part part = new Part();
            while (reader.Read())
            {
                if (!(reader.NodeType == XmlNodeType.EndElement && reader.Name.ToString() == "my:Parts"))
                {
                    if (reader.Name == "my:Part" || reader.Name == "my:PartsList")
                        currentNodeTypeText = reader.Name;
                    else
                    {
                        if (reader.NodeType == XmlNodeType.Element && currentNodeTypeText == "my:Part")
                        {
                            childNodeText = reader.Name;
                        }
                        if (reader.NodeType == XmlNodeType.Text && currentNodeTypeText == "my:Part" && part.GetType().GetProperty(childNodeText.Split(':')[1]) != null)
                            part.GetType().GetProperty(childNodeText.Split(':')[1]).SetValue(part, reader.Value);

                        else if (reader.NodeType == XmlNodeType.Text && currentNodeTypeText == "my:PartsList" && partNumberObj.GetType().GetProperty(currentNodeTypeText.Split(':')[1]) != null)
                            partNumberObj.GetType().GetProperty(currentNodeTypeText.Split(':')[1]).SetValue(partNumberObj, reader.Value);

                    }
                    if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "my:Part")
                        partNumberObj.Part.Add(part);
                }
                else
                {
                    Devform.Parts = partNumberObj;
                    return Devform;
                }
            }
            return null;
        }
        public static DeviationFields HyperlinkField(DeviationFields Devform, XmlTextReader reader)
        {
            string currentNodeTypeText = null;
            string parentNodeName = reader.Name.ToString();
            HyperlinkGroup link = new HyperlinkGroup() { HyperLinks = new HyperLinks() };
            while (reader.Read())
            {
                if (!(reader.NodeType == XmlNodeType.EndElement && reader.Name.ToString() == "my:HyperlinkGroup"))
                {
                    if (reader.Name == "my:Links")
                        currentNodeTypeText = reader.Name;
                    else
                    {
                        if (reader.NodeType == XmlNodeType.Text && currentNodeTypeText == "my:Links" && link.HyperLinks.GetType().GetProperty(currentNodeTypeText.Split(':')[1]) != null)
                        {
                            link.HyperLinks.GetType().GetProperty(currentNodeTypeText.Split(':')[1]).SetValue(link.HyperLinks, reader.Value);
                        }
                    }
                }
                else
                {
                    Devform.HyperlinkGroup = link;
                    return Devform;
                }
            }
            return null;
        }
        public static DeviationFields CheckAttachmentsCollection(DeviationFields Devform, XmlTextReader reader)
        {
            string currentNodeTypeText = null;
            string parentNodeName = reader.Name.ToString();
            Attachements attachements = new Attachements();
            while (reader.Read())
            {
                if (!(reader.NodeType == XmlNodeType.EndElement && reader.Name.ToString() == "my:FileAttachements"))
                {
                    if (reader.Name == "my:Attachment")
                        currentNodeTypeText = reader.Name;
                    else
                    {
                        if (reader.NodeType == XmlNodeType.Text && currentNodeTypeText == "my:Attachment" && attachements.GetType().GetProperty(currentNodeTypeText.Split(':')[1]) != null)
                        {
                            attachements.GetType().GetProperty(currentNodeTypeText.Split(':')[1]).SetValue(attachements, reader.Value);
                        }
                    }
                    if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "my:Attachment")
                        Devform.FileAttachements.Attachements.Add(attachements);
                }
                else
                {
                    return Devform;
                }
            }
            return null;
        }
        public static DeviationFields ShipmentStatusFields(DeviationFields Devform, XmlTextReader reader)
        {
            string currentNodeTypeText = null;
            string childNodeElement;
            XmlNodeType previousNodeType = XmlNodeType.None;
            string HtmlText = "";
            ShipmentStatus ShipmentStatusFields = new ShipmentStatus();
            while (reader.Read())
            {
                if (!(reader.NodeType == XmlNodeType.EndElement && reader.Name.ToString() == "my:ShipmentStatus"))
                {
                    if (reader.NodeType == XmlNodeType.Element && (reader.Name == "my:ShipComments" || reader.Name == "my:ShipStatus"))
                    {
                        currentNodeTypeText = reader.Name;
                    }
                    if (currentNodeTypeText == "my:ShipComments" && !(reader.NodeType == XmlNodeType.EndElement && reader.Name.ToString() == "my:ShipComments"))
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            childNodeElement = reader.Name;
                            previousNodeType = reader.NodeType;
                        }
                        else
                        {
                            if (reader.NodeType == XmlNodeType.Text && previousNodeType == XmlNodeType.Element && reader.Value != null)
                            {
                                HtmlText += reader.Value;
                            }
                        }
                    }

                    else
                    {
                        if (reader.NodeType == XmlNodeType.Text && currentNodeTypeText != "my:ShipComments" && ShipmentStatusFields.GetType().GetProperty(currentNodeTypeText.Split(':')[1]) != null)
                        {
                            ShipmentStatusFields.GetType().GetProperty(currentNodeTypeText.Split(':')[1]).SetValue(ShipmentStatusFields, reader.Value);
                        }
                    }
                }
                else
                {
                    Devform.ShipmentStatus = ShipmentStatusFields;
                    return Devform;
                }
            }
            return null;
        }
        public static DeviationFields MaterialSubstitutionFields(DeviationFields Devform, XmlTextReader reader)
        {
            string currentNodeTypeText = null;
            string parentNodeName = reader.Name.ToString();
            MaterialSubstitution MaterialSubstitutionFields = new MaterialSubstitution();
            while (reader.Read())
            {
                if (!(reader.NodeType == XmlNodeType.EndElement && reader.Name.ToString() == "my:MaterialSubstitution"))
                {
                    if (reader.NodeType == XmlNodeType.Element)
                        currentNodeTypeText = reader.Name;
                    else
                    {
                        if (reader.NodeType == XmlNodeType.Text && MaterialSubstitutionFields.GetType().GetProperty(currentNodeTypeText.Split(':')[1]) != null)
                        {
                            MaterialSubstitutionFields.GetType().GetProperty(currentNodeTypeText.Split(':')[1]).SetValue(MaterialSubstitutionFields, reader.Value);
                        }
                    }
                }
                else
                {
                    Devform.MaterialSubstitution = MaterialSubstitutionFields;
                    return Devform;
                }
            }
            return null;
        }
        public static DeviationFields DescriptionFields(DeviationFields Devform, XmlTextReader reader)
        {
            string currentNodeTypeText = null;
            XmlNodeType previousNodeType = XmlNodeType.None;
            Description DescriptionFields = new Description() { html = new Html() };
            var HtmlText = "";
            while (reader.Read())
            {
                if (!(reader.NodeType == XmlNodeType.EndElement && reader.Name.ToString() == "my:Description"))
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        currentNodeTypeText = reader.Name;
                        previousNodeType = reader.NodeType;
                    }
                    else
                    {
                        if (reader.NodeType == XmlNodeType.Text && previousNodeType == XmlNodeType.Element && reader.Value != null)
                        {
                            HtmlText += reader.Value;
                        }
                    }
                }
                else
                {
                    DescriptionFields.html.p = HtmlText;
                    Devform.Description = DescriptionFields;
                    return Devform;
                }
            }
            return null;
        }
        public static DeviationFields ReasonFields(DeviationFields Devform, XmlTextReader reader)
        {
            string currentNodeTypeText = null;
            XmlNodeType previousNodeType = XmlNodeType.None;
            Reason ReasonFields = new Reason() { html = new Html2() };
            string HtmlText = "";
            while (reader.Read())
            {
                if (!(reader.NodeType == XmlNodeType.EndElement && reader.Name.ToString() == "my:Reason"))
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        currentNodeTypeText = reader.Name;
                        previousNodeType = reader.NodeType;
                    }
                    else
                    {
                        if (reader.NodeType == XmlNodeType.Text && previousNodeType == XmlNodeType.Element && reader.Value != null)
                        {
                            HtmlText += reader.Value;
                        }
                    }
                }
                else
                {
                    ReasonFields.html.p = HtmlText;
                    Devform.Reason = ReasonFields;
                    return Devform;
                }
            }
            return null;
        }
        public static DeviationFields DistributionFields(DeviationFields Devform, XmlTextReader reader)
        {
            string currentNodeTypeText = null;
            string parentNodeName = reader.Name.ToString();
            string childNodeText = "";
            Distribution DistributionFields = new Distribution() { Person = new List<Person2>() };
            Person2 person = new Person2();
            while (reader.Read())
            {
                if (!(reader.NodeType == XmlNodeType.EndElement && reader.Name.ToString() == "my:Distribution"))
                {
                    if (reader.Name == "pc:Person" && reader.NodeType == XmlNodeType.Element)
                        currentNodeTypeText = reader.Name;
                    else
                    {
                        if (reader.NodeType == XmlNodeType.Element && currentNodeTypeText == "pc:Person")
                        {
                            childNodeText = reader.Name.Split(':')[1];
                        }
                        if (reader.NodeType == XmlNodeType.Text && currentNodeTypeText == "pc:Person" && person.GetType().GetProperty(childNodeText) != null)
                        {
                            person.GetType().GetProperty(childNodeText).SetValue(person, reader.Value);
                        }
                    }
                    if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "pc:Person")
                        DistributionFields.Person.Add(person);
                }
                else
                {
                    Devform.Distribution = DistributionFields;
                    return Devform;
                }
            }
            return null;
        }
     }
  }
