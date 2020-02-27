using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSOM.Test
{
    public class Part
    {

        public string PartNumber { get; set; }
        public string PartDescription { get; set; }
        public string PartRevision { get; set; }
        public string Quantity { get; set; }
    }

    public class Parts
    {
        public Parts()
        {
            this.Part = new List<Part>();
        }
        public List<Part> Part { get; set; }
        public string PartsList { get; set; }
    }

    public class Html
    {
        public string p { get; set; }
    }

    public class Description
    {
    public Html html { get; set; }
    }

    public class Html2
    {
        public string p { get; set; }
    }
    public class Reason
    {
        public Html2 html { get; set; }
    }

    public class Attachements
    {
        public string Attachment { get; set; }
    }

    public class FileAttachements
    {
        public List<Attachements> Attachements { get; set; }
    }

    public class Links
    {
        public string _Link { get; set; }
    }

    public class HyperLinks
    {
        public Links Links { get; set; }
    }

    public class HyperlinkGroup
    {
        public HyperLinks HyperLinks { get; set; }
    }

    public class ShipmentStatus
    {
        public string ShipStatus { get; set; }
        public string ShipComments { get; set; }
    }

    public class MaterialSubstitution
    {
        public string SubstitutionSupplier { get; set; }
        public string SubstitutedPartNumber { get; set; }
        public string SubstitutedPartDescription { get; set; }
        public string SubstitutedPartCost { get; set; }
        public string ReplacedPartCost { get; set; }
        public string CARRequired { get; set; }
    }

    public class Person
    {
        public string DisplayName { get; set; }
        public string AccountId { get; set; }
        public string AccountType { get; set; }
    }

    public class Name
    {
        public Person Person { get; set; }
    }

    public class Approval
    {
        public string Function { get; set; }
        public Name Name { get; set; }
    }

    public class Approvals
    {
        public Approval Approval { get; set; }
    }

    public class Person2
    {
        public string DisplayName { get; set; }
        public string AccountId { get; set; }
        public string AccountType { get; set; }
    }

    public class Distribution
    {
        public List<Person2> Person { get; set; }
    }

    public class DeviationFields
    {
        public string Title { get; set; }
        public string RequestSite { get; set; }
        public string RequestNumber { get; set; }
        public string Date { get; set; }
        public string Originator { get; set; }
        public string DeviationType { get; set; }
        public string ExpirationDate { get; set; }
        public string OrganizationId { get; set; }
        public Parts Parts { get; set; }
        public Description Description { get; set; }
        public Reason Reason { get; set; }
        public FileAttachements FileAttachements { get; set; }
        public HyperlinkGroup HyperlinkGroup { get; set; }
        public ShipmentStatus ShipmentStatus { get; set; }
        public MaterialSubstitution MaterialSubstitution { get; set; }
        public List<Approvals> Approvals { get; set; }
        public Distribution Distribution { get; set; }
        public string DeviationApprovers { get; set; }
        public string DeviationDistributionGroup { get; set; }
    }

    public class RootObject
    {
        public DeviationFields myFields { get; set; }
    }

}
