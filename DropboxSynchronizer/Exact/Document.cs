using System;
using DropboxSynchronizer.Interfaces;
using Newtonsoft.Json;

namespace DropboxSynchronizer.Exact
{
    /// <summary>
    /// Exact document representation.
    /// </summary>
    internal class Document : IDocument
    {
        [JsonProperty("Account")]
        public object Account { get; set; }

        [JsonProperty("AccountCode")]
        public object AccountCode { get; set; }

        [JsonProperty("AccountName")]
        public object AccountName { get; set; }

        [JsonProperty("AmountFC")]
        public object AmountFC { get; set; }

        [JsonProperty("Body")]
        public object Body { get; set; }

        [JsonProperty("Category")]
        public object Category { get; set; }

        [JsonProperty("CategoryDescription")]
        public object CategoryDescription { get; set; }

        [JsonProperty("Created")]
        public DateTime Created { get; set; }

        [JsonProperty("Creator")]
        public string Creator { get; set; }

        [JsonProperty("CreatorFullName")]
        public string CreatorFullName { get; set; }

        [JsonProperty("Currency")]
        public string Currency { get; set; }

        [JsonProperty("Division")]
        public int Division { get; set; }

        [JsonProperty("DocumentDate")]
        public DateTime DocumentDate { get; set; }

        [JsonProperty("DocumentViewUrl")]
        public string DocumentViewUrl { get; set; }

        [JsonProperty("FinancialTransactionEntryID")]
        public object FinancialTransactionEntryID { get; set; }

        [JsonProperty("HasEmptyBody")]
        public bool HasEmptyBody { get; set; }

        [JsonProperty("HID")]
        public int HID { get; set; }

        [JsonProperty("ID")]
        public string ID { get; set; }

        [JsonProperty("Language")]
        public string Language { get; set; }

        [JsonProperty("Modified")]
        public DateTime Modified { get; set; }

        [JsonProperty("Modifier")]
        public string Modifier { get; set; }

        [JsonProperty("ModifierFullName")]
        public string ModifierFullName { get; set; }

        [JsonProperty("ShopOrderNumber")]
        public object ShopOrderNumber { get; set; }

        [JsonProperty("Subject")]
        public string Subject { get; set; }

        [JsonProperty("Type")]
        public int Type { get; set; }

        [JsonProperty("TypeDescription")]
        public string TypeDescription { get; set; }

        [JsonProperty("Opportunity")]
        public object Opportunity { get; set; }

        [JsonProperty("SalesInvoiceNumber")]
        public object SalesInvoiceNumber { get; set; }
    }
}
