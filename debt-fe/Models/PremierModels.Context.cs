﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace debt_fe.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class PremierEntities : DbContext
    {
        public PremierEntities()
            : base("name=PremierEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Vw_PremierMessage> Vw_PremierMessage { get; set; }
        public virtual DbSet<PremierMessage> PremierMessages { get; set; }
        public virtual DbSet<Vw_TroubleTicket> Vw_TroubleTicket { get; set; }
        public virtual DbSet<Vw_DebtExt_Creditor> Vw_DebtExt_Creditor { get; set; }
        public virtual DbSet<Vw_PremierActivity> Vw_PremierActivity { get; set; }
    
        public virtual int xp_debtext_client_insupd2(Nullable<int> memberISN, Nullable<int> salemanISN, Nullable<int> campaignISN, string memUserName, string memPassword, string memFax, string memFirstName, string memLastName, string memPhone, string memEmail, string memAddress, string memZip, string memCity, string memState, string memCompanyName, string memTitle, string memIM, Nullable<byte> memStatus, string memComment, Nullable<decimal> memCreditLine, Nullable<int> dealerISN, string aNI, Nullable<double> memCreditScore, Nullable<decimal> memApprovalPayment, Nullable<decimal> memApprovalAmount, string memSSN, Nullable<byte> memConnectedLO, Nullable<int> updatedBy, Nullable<byte> memTradeVehicle, string memYearVehicle, string memManufacturerVehicle, string memModelVehicle, Nullable<decimal> memMonthlyPayment, string memPolycomID, string memWeekDay, Nullable<decimal> memHomeValue, Nullable<decimal> memCurrentLeftOnLoan, Nullable<double> memInterestRate, Nullable<decimal> memMonthlyPaymentM, Nullable<decimal> memAppoxTotalDebt, string memTypeOfLoan, string memAnyLatePayment, string memApprovalOfRCredit, string memIsYourCreditGreat, string memTimeZoneFull, string memDomainName, Nullable<decimal> memPayOffAmount, Nullable<decimal> balance, string ratePlan, Nullable<System.DateTime> memDOB, Nullable<int> lastStepFlag, Nullable<byte> memReadyPurchaseNow, string xmlInfoExt, Nullable<int> phoneRechargeNo, string memDropID, Nullable<int> accountManagerISN)
        {
            var memberISNParameter = memberISN.HasValue ?
                new ObjectParameter("MemberISN", memberISN) :
                new ObjectParameter("MemberISN", typeof(int));
    
            var salemanISNParameter = salemanISN.HasValue ?
                new ObjectParameter("SalemanISN", salemanISN) :
                new ObjectParameter("SalemanISN", typeof(int));
    
            var campaignISNParameter = campaignISN.HasValue ?
                new ObjectParameter("CampaignISN", campaignISN) :
                new ObjectParameter("CampaignISN", typeof(int));
    
            var memUserNameParameter = memUserName != null ?
                new ObjectParameter("memUserName", memUserName) :
                new ObjectParameter("memUserName", typeof(string));
    
            var memPasswordParameter = memPassword != null ?
                new ObjectParameter("memPassword", memPassword) :
                new ObjectParameter("memPassword", typeof(string));
    
            var memFaxParameter = memFax != null ?
                new ObjectParameter("memFax", memFax) :
                new ObjectParameter("memFax", typeof(string));
    
            var memFirstNameParameter = memFirstName != null ?
                new ObjectParameter("memFirstName", memFirstName) :
                new ObjectParameter("memFirstName", typeof(string));
    
            var memLastNameParameter = memLastName != null ?
                new ObjectParameter("memLastName", memLastName) :
                new ObjectParameter("memLastName", typeof(string));
    
            var memPhoneParameter = memPhone != null ?
                new ObjectParameter("memPhone", memPhone) :
                new ObjectParameter("memPhone", typeof(string));
    
            var memEmailParameter = memEmail != null ?
                new ObjectParameter("memEmail", memEmail) :
                new ObjectParameter("memEmail", typeof(string));
    
            var memAddressParameter = memAddress != null ?
                new ObjectParameter("memAddress", memAddress) :
                new ObjectParameter("memAddress", typeof(string));
    
            var memZipParameter = memZip != null ?
                new ObjectParameter("memZip", memZip) :
                new ObjectParameter("memZip", typeof(string));
    
            var memCityParameter = memCity != null ?
                new ObjectParameter("memCity", memCity) :
                new ObjectParameter("memCity", typeof(string));
    
            var memStateParameter = memState != null ?
                new ObjectParameter("memState", memState) :
                new ObjectParameter("memState", typeof(string));
    
            var memCompanyNameParameter = memCompanyName != null ?
                new ObjectParameter("memCompanyName", memCompanyName) :
                new ObjectParameter("memCompanyName", typeof(string));
    
            var memTitleParameter = memTitle != null ?
                new ObjectParameter("memTitle", memTitle) :
                new ObjectParameter("memTitle", typeof(string));
    
            var memIMParameter = memIM != null ?
                new ObjectParameter("memIM", memIM) :
                new ObjectParameter("memIM", typeof(string));
    
            var memStatusParameter = memStatus.HasValue ?
                new ObjectParameter("memStatus", memStatus) :
                new ObjectParameter("memStatus", typeof(byte));
    
            var memCommentParameter = memComment != null ?
                new ObjectParameter("memComment", memComment) :
                new ObjectParameter("memComment", typeof(string));
    
            var memCreditLineParameter = memCreditLine.HasValue ?
                new ObjectParameter("memCreditLine", memCreditLine) :
                new ObjectParameter("memCreditLine", typeof(decimal));
    
            var dealerISNParameter = dealerISN.HasValue ?
                new ObjectParameter("DealerISN", dealerISN) :
                new ObjectParameter("DealerISN", typeof(int));
    
            var aNIParameter = aNI != null ?
                new ObjectParameter("ANI", aNI) :
                new ObjectParameter("ANI", typeof(string));
    
            var memCreditScoreParameter = memCreditScore.HasValue ?
                new ObjectParameter("memCreditScore", memCreditScore) :
                new ObjectParameter("memCreditScore", typeof(double));
    
            var memApprovalPaymentParameter = memApprovalPayment.HasValue ?
                new ObjectParameter("memApprovalPayment", memApprovalPayment) :
                new ObjectParameter("memApprovalPayment", typeof(decimal));
    
            var memApprovalAmountParameter = memApprovalAmount.HasValue ?
                new ObjectParameter("memApprovalAmount", memApprovalAmount) :
                new ObjectParameter("memApprovalAmount", typeof(decimal));
    
            var memSSNParameter = memSSN != null ?
                new ObjectParameter("memSSN", memSSN) :
                new ObjectParameter("memSSN", typeof(string));
    
            var memConnectedLOParameter = memConnectedLO.HasValue ?
                new ObjectParameter("memConnectedLO", memConnectedLO) :
                new ObjectParameter("memConnectedLO", typeof(byte));
    
            var updatedByParameter = updatedBy.HasValue ?
                new ObjectParameter("updatedBy", updatedBy) :
                new ObjectParameter("updatedBy", typeof(int));
    
            var memTradeVehicleParameter = memTradeVehicle.HasValue ?
                new ObjectParameter("memTradeVehicle", memTradeVehicle) :
                new ObjectParameter("memTradeVehicle", typeof(byte));
    
            var memYearVehicleParameter = memYearVehicle != null ?
                new ObjectParameter("memYearVehicle", memYearVehicle) :
                new ObjectParameter("memYearVehicle", typeof(string));
    
            var memManufacturerVehicleParameter = memManufacturerVehicle != null ?
                new ObjectParameter("memManufacturerVehicle", memManufacturerVehicle) :
                new ObjectParameter("memManufacturerVehicle", typeof(string));
    
            var memModelVehicleParameter = memModelVehicle != null ?
                new ObjectParameter("memModelVehicle", memModelVehicle) :
                new ObjectParameter("memModelVehicle", typeof(string));
    
            var memMonthlyPaymentParameter = memMonthlyPayment.HasValue ?
                new ObjectParameter("memMonthlyPayment", memMonthlyPayment) :
                new ObjectParameter("memMonthlyPayment", typeof(decimal));
    
            var memPolycomIDParameter = memPolycomID != null ?
                new ObjectParameter("memPolycomID", memPolycomID) :
                new ObjectParameter("memPolycomID", typeof(string));
    
            var memWeekDayParameter = memWeekDay != null ?
                new ObjectParameter("memWeekDay", memWeekDay) :
                new ObjectParameter("memWeekDay", typeof(string));
    
            var memHomeValueParameter = memHomeValue.HasValue ?
                new ObjectParameter("memHomeValue", memHomeValue) :
                new ObjectParameter("memHomeValue", typeof(decimal));
    
            var memCurrentLeftOnLoanParameter = memCurrentLeftOnLoan.HasValue ?
                new ObjectParameter("memCurrentLeftOnLoan", memCurrentLeftOnLoan) :
                new ObjectParameter("memCurrentLeftOnLoan", typeof(decimal));
    
            var memInterestRateParameter = memInterestRate.HasValue ?
                new ObjectParameter("memInterestRate", memInterestRate) :
                new ObjectParameter("memInterestRate", typeof(double));
    
            var memMonthlyPaymentMParameter = memMonthlyPaymentM.HasValue ?
                new ObjectParameter("memMonthlyPaymentM", memMonthlyPaymentM) :
                new ObjectParameter("memMonthlyPaymentM", typeof(decimal));
    
            var memAppoxTotalDebtParameter = memAppoxTotalDebt.HasValue ?
                new ObjectParameter("memAppoxTotalDebt", memAppoxTotalDebt) :
                new ObjectParameter("memAppoxTotalDebt", typeof(decimal));
    
            var memTypeOfLoanParameter = memTypeOfLoan != null ?
                new ObjectParameter("memTypeOfLoan", memTypeOfLoan) :
                new ObjectParameter("memTypeOfLoan", typeof(string));
    
            var memAnyLatePaymentParameter = memAnyLatePayment != null ?
                new ObjectParameter("memAnyLatePayment", memAnyLatePayment) :
                new ObjectParameter("memAnyLatePayment", typeof(string));
    
            var memApprovalOfRCreditParameter = memApprovalOfRCredit != null ?
                new ObjectParameter("memApprovalOfRCredit", memApprovalOfRCredit) :
                new ObjectParameter("memApprovalOfRCredit", typeof(string));
    
            var memIsYourCreditGreatParameter = memIsYourCreditGreat != null ?
                new ObjectParameter("memIsYourCreditGreat", memIsYourCreditGreat) :
                new ObjectParameter("memIsYourCreditGreat", typeof(string));
    
            var memTimeZoneFullParameter = memTimeZoneFull != null ?
                new ObjectParameter("memTimeZoneFull", memTimeZoneFull) :
                new ObjectParameter("memTimeZoneFull", typeof(string));
    
            var memDomainNameParameter = memDomainName != null ?
                new ObjectParameter("memDomainName", memDomainName) :
                new ObjectParameter("memDomainName", typeof(string));
    
            var memPayOffAmountParameter = memPayOffAmount.HasValue ?
                new ObjectParameter("memPayOffAmount", memPayOffAmount) :
                new ObjectParameter("memPayOffAmount", typeof(decimal));
    
            var balanceParameter = balance.HasValue ?
                new ObjectParameter("Balance", balance) :
                new ObjectParameter("Balance", typeof(decimal));
    
            var ratePlanParameter = ratePlan != null ?
                new ObjectParameter("RatePlan", ratePlan) :
                new ObjectParameter("RatePlan", typeof(string));
    
            var memDOBParameter = memDOB.HasValue ?
                new ObjectParameter("memDOB", memDOB) :
                new ObjectParameter("memDOB", typeof(System.DateTime));
    
            var lastStepFlagParameter = lastStepFlag.HasValue ?
                new ObjectParameter("LastStepFlag", lastStepFlag) :
                new ObjectParameter("LastStepFlag", typeof(int));
    
            var memReadyPurchaseNowParameter = memReadyPurchaseNow.HasValue ?
                new ObjectParameter("memReadyPurchaseNow", memReadyPurchaseNow) :
                new ObjectParameter("memReadyPurchaseNow", typeof(byte));
    
            var xmlInfoExtParameter = xmlInfoExt != null ?
                new ObjectParameter("xmlInfoExt", xmlInfoExt) :
                new ObjectParameter("xmlInfoExt", typeof(string));
    
            var phoneRechargeNoParameter = phoneRechargeNo.HasValue ?
                new ObjectParameter("PhoneRechargeNo", phoneRechargeNo) :
                new ObjectParameter("PhoneRechargeNo", typeof(int));
    
            var memDropIDParameter = memDropID != null ?
                new ObjectParameter("memDropID", memDropID) :
                new ObjectParameter("memDropID", typeof(string));
    
            var accountManagerISNParameter = accountManagerISN.HasValue ?
                new ObjectParameter("AccountManagerISN", accountManagerISN) :
                new ObjectParameter("AccountManagerISN", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("xp_debtext_client_insupd2", memberISNParameter, salemanISNParameter, campaignISNParameter, memUserNameParameter, memPasswordParameter, memFaxParameter, memFirstNameParameter, memLastNameParameter, memPhoneParameter, memEmailParameter, memAddressParameter, memZipParameter, memCityParameter, memStateParameter, memCompanyNameParameter, memTitleParameter, memIMParameter, memStatusParameter, memCommentParameter, memCreditLineParameter, dealerISNParameter, aNIParameter, memCreditScoreParameter, memApprovalPaymentParameter, memApprovalAmountParameter, memSSNParameter, memConnectedLOParameter, updatedByParameter, memTradeVehicleParameter, memYearVehicleParameter, memManufacturerVehicleParameter, memModelVehicleParameter, memMonthlyPaymentParameter, memPolycomIDParameter, memWeekDayParameter, memHomeValueParameter, memCurrentLeftOnLoanParameter, memInterestRateParameter, memMonthlyPaymentMParameter, memAppoxTotalDebtParameter, memTypeOfLoanParameter, memAnyLatePaymentParameter, memApprovalOfRCreditParameter, memIsYourCreditGreatParameter, memTimeZoneFullParameter, memDomainNameParameter, memPayOffAmountParameter, balanceParameter, ratePlanParameter, memDOBParameter, lastStepFlagParameter, memReadyPurchaseNowParameter, xmlInfoExtParameter, phoneRechargeNoParameter, memDropIDParameter, accountManagerISNParameter);
        }
    
        public virtual ObjectResult<xp_debtuser_getinfo_Result> xp_debtuser_getinfo(Nullable<int> memberISN)
        {
            var memberISNParameter = memberISN.HasValue ?
                new ObjectParameter("MemberISN", memberISN) :
                new ObjectParameter("MemberISN", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<xp_debtuser_getinfo_Result>("xp_debtuser_getinfo", memberISNParameter);
        }
    
        public virtual int xp_premiermessage_new(Nullable<int> memberISN, string subject, string content, Nullable<int> updatedBy)
        {
            var memberISNParameter = memberISN.HasValue ?
                new ObjectParameter("MemberISN", memberISN) :
                new ObjectParameter("MemberISN", typeof(int));
    
            var subjectParameter = subject != null ?
                new ObjectParameter("Subject", subject) :
                new ObjectParameter("Subject", typeof(string));
    
            var contentParameter = content != null ?
                new ObjectParameter("Content", content) :
                new ObjectParameter("Content", typeof(string));
    
            var updatedByParameter = updatedBy.HasValue ?
                new ObjectParameter("updatedBy", updatedBy) :
                new ObjectParameter("updatedBy", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("xp_premiermessage_new", memberISNParameter, subjectParameter, contentParameter, updatedByParameter);
        }
    
        public virtual int xp_premiermessage_reply(Nullable<int> messageISN, string content, Nullable<int> updatedBy)
        {
            var messageISNParameter = messageISN.HasValue ?
                new ObjectParameter("MessageISN", messageISN) :
                new ObjectParameter("MessageISN", typeof(int));
    
            var contentParameter = content != null ?
                new ObjectParameter("Content", content) :
                new ObjectParameter("Content", typeof(string));
    
            var updatedByParameter = updatedBy.HasValue ?
                new ObjectParameter("updatedBy", updatedBy) :
                new ObjectParameter("updatedBy", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("xp_premiermessage_reply", messageISNParameter, contentParameter, updatedByParameter);
        }
    
        public virtual ObjectResult<xp_premiermessage_viewall_Result> xp_premiermessage_viewall(Nullable<int> messageISN)
        {
            var messageISNParameter = messageISN.HasValue ?
                new ObjectParameter("MessageISN", messageISN) :
                new ObjectParameter("MessageISN", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<xp_premiermessage_viewall_Result>("xp_premiermessage_viewall", messageISNParameter);
        }
    
        public virtual int xp_client_profile_requestchange(Nullable<int> memberISN, string content)
        {
            var memberISNParameter = memberISN.HasValue ?
                new ObjectParameter("MemberISN", memberISN) :
                new ObjectParameter("MemberISN", typeof(int));
    
            var contentParameter = content != null ?
                new ObjectParameter("Content", content) :
                new ObjectParameter("Content", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("xp_client_profile_requestchange", memberISNParameter, contentParameter);
        }
    
        public virtual int xp_complaint_ins(Nullable<int> memberISN, string complaint, Nullable<int> addedBy)
        {
            var memberISNParameter = memberISN.HasValue ?
                new ObjectParameter("MemberISN", memberISN) :
                new ObjectParameter("MemberISN", typeof(int));
    
            var complaintParameter = complaint != null ?
                new ObjectParameter("Complaint", complaint) :
                new ObjectParameter("Complaint", typeof(string));
    
            var addedByParameter = addedBy.HasValue ?
                new ObjectParameter("addedBy", addedBy) :
                new ObjectParameter("addedBy", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("xp_complaint_ins", memberISNParameter, complaintParameter, addedByParameter);
        }
    
        public virtual int xp_debtext_document_signature_upd(Nullable<int> documentISN, string docFileName, Nullable<int> docSize, Nullable<System.DateTime> docSignatureDate, string docSignatureIP, string clientInfo, Nullable<int> updatedBy)
        {
            var documentISNParameter = documentISN.HasValue ?
                new ObjectParameter("DocumentISN", documentISN) :
                new ObjectParameter("DocumentISN", typeof(int));
    
            var docFileNameParameter = docFileName != null ?
                new ObjectParameter("docFileName", docFileName) :
                new ObjectParameter("docFileName", typeof(string));
    
            var docSizeParameter = docSize.HasValue ?
                new ObjectParameter("docSize", docSize) :
                new ObjectParameter("docSize", typeof(int));
    
            var docSignatureDateParameter = docSignatureDate.HasValue ?
                new ObjectParameter("docSignatureDate", docSignatureDate) :
                new ObjectParameter("docSignatureDate", typeof(System.DateTime));
    
            var docSignatureIPParameter = docSignatureIP != null ?
                new ObjectParameter("docSignatureIP", docSignatureIP) :
                new ObjectParameter("docSignatureIP", typeof(string));
    
            var clientInfoParameter = clientInfo != null ?
                new ObjectParameter("ClientInfo", clientInfo) :
                new ObjectParameter("ClientInfo", typeof(string));
    
            var updatedByParameter = updatedBy.HasValue ?
                new ObjectParameter("updatedBy", updatedBy) :
                new ObjectParameter("updatedBy", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("xp_debtext_document_signature_upd", documentISNParameter, docFileNameParameter, docSizeParameter, docSignatureDateParameter, docSignatureIPParameter, clientInfoParameter, updatedByParameter);
        }
    
        public virtual int xp_member_changepwd(Nullable<int> memberISN, string newPassword, Nullable<int> updatedBy)
        {
            var memberISNParameter = memberISN.HasValue ?
                new ObjectParameter("MemberISN", memberISN) :
                new ObjectParameter("MemberISN", typeof(int));
    
            var newPasswordParameter = newPassword != null ?
                new ObjectParameter("NewPassword", newPassword) :
                new ObjectParameter("NewPassword", typeof(string));
    
            var updatedByParameter = updatedBy.HasValue ?
                new ObjectParameter("updatedBy", updatedBy) :
                new ObjectParameter("updatedBy", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("xp_member_changepwd", memberISNParameter, newPasswordParameter, updatedByParameter);
        }
    
        public virtual int xp_debtext_client_password_upd(Nullable<int> memberISN, string memPassword, Nullable<int> updatedBy)
        {
            var memberISNParameter = memberISN.HasValue ?
                new ObjectParameter("MemberISN", memberISN) :
                new ObjectParameter("MemberISN", typeof(int));
    
            var memPasswordParameter = memPassword != null ?
                new ObjectParameter("memPassword", memPassword) :
                new ObjectParameter("memPassword", typeof(string));
    
            var updatedByParameter = updatedBy.HasValue ?
                new ObjectParameter("updatedBy", updatedBy) :
                new ObjectParameter("updatedBy", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("xp_debtext_client_password_upd", memberISNParameter, memPasswordParameter, updatedByParameter);
        }
    }
}
