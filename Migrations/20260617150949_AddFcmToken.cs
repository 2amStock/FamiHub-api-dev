using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamiHub.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFcmToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FamilyEvents_Families_FamilyId",
                table: "FamilyEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_FamilyEvents_Users_CreatedByUserId",
                table: "FamilyEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_MealSuggestions_Families_FamilyId",
                table: "MealSuggestions");

            migrationBuilder.DropForeignKey(
                name: "FK_MealSuggestions_Users_RequestedByUserId",
                table: "MealSuggestions");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_SubscriptionPlans_SubscriptionPlanId",
                table: "PaymentTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_Users_UserId",
                table: "PaymentTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_RewardRedemptions_Rewards_RewardId",
                table: "RewardRedemptions");

            migrationBuilder.DropForeignKey(
                name: "FK_RewardRedemptions_Users_ChildUserId",
                table: "RewardRedemptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Rewards_Families_FamilyId",
                table: "Rewards");

            migrationBuilder.DropForeignKey(
                name: "FK_Rewards_Users_CreatedByUserId",
                table: "Rewards");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskProofs_Tasks_TaskId",
                table: "TaskProofs");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskProofs_Users_ChildUserId",
                table: "TaskProofs");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Families_FamilyId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_AssignedToUserId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_CreatedByUserId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFoodPreferences_Users_UserId",
                table: "UserFoodPreferences");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Families_FamilyId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptions_SubscriptionPlans_SubscriptionPlanId",
                table: "UserSubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptions_Users_UserId",
                table: "UserSubscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSubscriptions",
                table: "UserSubscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFoodPreferences",
                table: "UserFoodPreferences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskProofs",
                table: "TaskProofs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubscriptionPlans",
                table: "SubscriptionPlans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Rewards",
                table: "Rewards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RewardRedemptions",
                table: "RewardRedemptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentTransactions",
                table: "PaymentTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MealSuggestions",
                table: "MealSuggestions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FamilyEvents",
                table: "FamilyEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Families",
                table: "Families");

            migrationBuilder.RenameTable(
                name: "UserSubscriptions",
                newName: "usersubscriptions");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "UserFoodPreferences",
                newName: "userfoodpreferences");

            migrationBuilder.RenameTable(
                name: "Tasks",
                newName: "tasks");

            migrationBuilder.RenameTable(
                name: "TaskProofs",
                newName: "taskproofs");

            migrationBuilder.RenameTable(
                name: "SubscriptionPlans",
                newName: "subscriptionplans");

            migrationBuilder.RenameTable(
                name: "Rewards",
                newName: "rewards");

            migrationBuilder.RenameTable(
                name: "RewardRedemptions",
                newName: "rewardredemptions");

            migrationBuilder.RenameTable(
                name: "PaymentTransactions",
                newName: "paymenttransactions");

            migrationBuilder.RenameTable(
                name: "Notifications",
                newName: "notifications");

            migrationBuilder.RenameTable(
                name: "MealSuggestions",
                newName: "mealsuggestions");

            migrationBuilder.RenameTable(
                name: "FamilyEvents",
                newName: "familyevents");

            migrationBuilder.RenameTable(
                name: "Families",
                newName: "families");

            migrationBuilder.RenameIndex(
                name: "IX_UserSubscriptions_UserId",
                table: "usersubscriptions",
                newName: "IX_usersubscriptions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserSubscriptions_SubscriptionPlanId",
                table: "usersubscriptions",
                newName: "IX_usersubscriptions_SubscriptionPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_FamilyId",
                table: "users",
                newName: "IX_users_FamilyId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                table: "users",
                newName: "IX_users_Email");

            migrationBuilder.RenameIndex(
                name: "IX_UserFoodPreferences_UserId",
                table: "userfoodpreferences",
                newName: "IX_userfoodpreferences_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_FamilyId",
                table: "tasks",
                newName: "IX_tasks_FamilyId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_CreatedByUserId",
                table: "tasks",
                newName: "IX_tasks_CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_AssignedToUserId",
                table: "tasks",
                newName: "IX_tasks_AssignedToUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskProofs_TaskId",
                table: "taskproofs",
                newName: "IX_taskproofs_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskProofs_ChildUserId",
                table: "taskproofs",
                newName: "IX_taskproofs_ChildUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Rewards_FamilyId",
                table: "rewards",
                newName: "IX_rewards_FamilyId");

            migrationBuilder.RenameIndex(
                name: "IX_Rewards_CreatedByUserId",
                table: "rewards",
                newName: "IX_rewards_CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_RewardRedemptions_RewardId",
                table: "rewardredemptions",
                newName: "IX_rewardredemptions_RewardId");

            migrationBuilder.RenameIndex(
                name: "IX_RewardRedemptions_ChildUserId",
                table: "rewardredemptions",
                newName: "IX_rewardredemptions_ChildUserId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentTransactions_UserId",
                table: "paymenttransactions",
                newName: "IX_paymenttransactions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentTransactions_SubscriptionPlanId",
                table: "paymenttransactions",
                newName: "IX_paymenttransactions_SubscriptionPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_UserId",
                table: "notifications",
                newName: "IX_notifications_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_MealSuggestions_RequestedByUserId",
                table: "mealsuggestions",
                newName: "IX_mealsuggestions_RequestedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_MealSuggestions_FamilyId",
                table: "mealsuggestions",
                newName: "IX_mealsuggestions_FamilyId");

            migrationBuilder.RenameIndex(
                name: "IX_FamilyEvents_FamilyId",
                table: "familyevents",
                newName: "IX_familyevents_FamilyId");

            migrationBuilder.RenameIndex(
                name: "IX_FamilyEvents_CreatedByUserId",
                table: "familyevents",
                newName: "IX_familyevents_CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Families_InviteCode",
                table: "families",
                newName: "IX_families_InviteCode");

            migrationBuilder.AddColumn<string>(
                name: "FcmToken",
                table: "users",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_usersubscriptions",
                table: "usersubscriptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_userfoodpreferences",
                table: "userfoodpreferences",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tasks",
                table: "tasks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_taskproofs",
                table: "taskproofs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_subscriptionplans",
                table: "subscriptionplans",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_rewards",
                table: "rewards",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_rewardredemptions",
                table: "rewardredemptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_paymenttransactions",
                table: "paymenttransactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_notifications",
                table: "notifications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_mealsuggestions",
                table: "mealsuggestions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_familyevents",
                table: "familyevents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_families",
                table: "families",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_familyevents_families_FamilyId",
                table: "familyevents",
                column: "FamilyId",
                principalTable: "families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_familyevents_users_CreatedByUserId",
                table: "familyevents",
                column: "CreatedByUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_mealsuggestions_families_FamilyId",
                table: "mealsuggestions",
                column: "FamilyId",
                principalTable: "families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_mealsuggestions_users_RequestedByUserId",
                table: "mealsuggestions",
                column: "RequestedByUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_users_UserId",
                table: "notifications",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_paymenttransactions_subscriptionplans_SubscriptionPlanId",
                table: "paymenttransactions",
                column: "SubscriptionPlanId",
                principalTable: "subscriptionplans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_paymenttransactions_users_UserId",
                table: "paymenttransactions",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_rewardredemptions_rewards_RewardId",
                table: "rewardredemptions",
                column: "RewardId",
                principalTable: "rewards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_rewardredemptions_users_ChildUserId",
                table: "rewardredemptions",
                column: "ChildUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_rewards_families_FamilyId",
                table: "rewards",
                column: "FamilyId",
                principalTable: "families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_rewards_users_CreatedByUserId",
                table: "rewards",
                column: "CreatedByUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_taskproofs_tasks_TaskId",
                table: "taskproofs",
                column: "TaskId",
                principalTable: "tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_taskproofs_users_ChildUserId",
                table: "taskproofs",
                column: "ChildUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tasks_families_FamilyId",
                table: "tasks",
                column: "FamilyId",
                principalTable: "families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tasks_users_AssignedToUserId",
                table: "tasks",
                column: "AssignedToUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_tasks_users_CreatedByUserId",
                table: "tasks",
                column: "CreatedByUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_userfoodpreferences_users_UserId",
                table: "userfoodpreferences",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_users_families_FamilyId",
                table: "users",
                column: "FamilyId",
                principalTable: "families",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_usersubscriptions_subscriptionplans_SubscriptionPlanId",
                table: "usersubscriptions",
                column: "SubscriptionPlanId",
                principalTable: "subscriptionplans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_usersubscriptions_users_UserId",
                table: "usersubscriptions",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_familyevents_families_FamilyId",
                table: "familyevents");

            migrationBuilder.DropForeignKey(
                name: "FK_familyevents_users_CreatedByUserId",
                table: "familyevents");

            migrationBuilder.DropForeignKey(
                name: "FK_mealsuggestions_families_FamilyId",
                table: "mealsuggestions");

            migrationBuilder.DropForeignKey(
                name: "FK_mealsuggestions_users_RequestedByUserId",
                table: "mealsuggestions");

            migrationBuilder.DropForeignKey(
                name: "FK_notifications_users_UserId",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_paymenttransactions_subscriptionplans_SubscriptionPlanId",
                table: "paymenttransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_paymenttransactions_users_UserId",
                table: "paymenttransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_rewardredemptions_rewards_RewardId",
                table: "rewardredemptions");

            migrationBuilder.DropForeignKey(
                name: "FK_rewardredemptions_users_ChildUserId",
                table: "rewardredemptions");

            migrationBuilder.DropForeignKey(
                name: "FK_rewards_families_FamilyId",
                table: "rewards");

            migrationBuilder.DropForeignKey(
                name: "FK_rewards_users_CreatedByUserId",
                table: "rewards");

            migrationBuilder.DropForeignKey(
                name: "FK_taskproofs_tasks_TaskId",
                table: "taskproofs");

            migrationBuilder.DropForeignKey(
                name: "FK_taskproofs_users_ChildUserId",
                table: "taskproofs");

            migrationBuilder.DropForeignKey(
                name: "FK_tasks_families_FamilyId",
                table: "tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_tasks_users_AssignedToUserId",
                table: "tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_tasks_users_CreatedByUserId",
                table: "tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_userfoodpreferences_users_UserId",
                table: "userfoodpreferences");

            migrationBuilder.DropForeignKey(
                name: "FK_users_families_FamilyId",
                table: "users");

            migrationBuilder.DropForeignKey(
                name: "FK_usersubscriptions_subscriptionplans_SubscriptionPlanId",
                table: "usersubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_usersubscriptions_users_UserId",
                table: "usersubscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_usersubscriptions",
                table: "usersubscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_userfoodpreferences",
                table: "userfoodpreferences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tasks",
                table: "tasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_taskproofs",
                table: "taskproofs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_subscriptionplans",
                table: "subscriptionplans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_rewards",
                table: "rewards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_rewardredemptions",
                table: "rewardredemptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_paymenttransactions",
                table: "paymenttransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_notifications",
                table: "notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_mealsuggestions",
                table: "mealsuggestions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_familyevents",
                table: "familyevents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_families",
                table: "families");

            migrationBuilder.DropColumn(
                name: "FcmToken",
                table: "users");

            migrationBuilder.RenameTable(
                name: "usersubscriptions",
                newName: "UserSubscriptions");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "userfoodpreferences",
                newName: "UserFoodPreferences");

            migrationBuilder.RenameTable(
                name: "tasks",
                newName: "Tasks");

            migrationBuilder.RenameTable(
                name: "taskproofs",
                newName: "TaskProofs");

            migrationBuilder.RenameTable(
                name: "subscriptionplans",
                newName: "SubscriptionPlans");

            migrationBuilder.RenameTable(
                name: "rewards",
                newName: "Rewards");

            migrationBuilder.RenameTable(
                name: "rewardredemptions",
                newName: "RewardRedemptions");

            migrationBuilder.RenameTable(
                name: "paymenttransactions",
                newName: "PaymentTransactions");

            migrationBuilder.RenameTable(
                name: "notifications",
                newName: "Notifications");

            migrationBuilder.RenameTable(
                name: "mealsuggestions",
                newName: "MealSuggestions");

            migrationBuilder.RenameTable(
                name: "familyevents",
                newName: "FamilyEvents");

            migrationBuilder.RenameTable(
                name: "families",
                newName: "Families");

            migrationBuilder.RenameIndex(
                name: "IX_usersubscriptions_UserId",
                table: "UserSubscriptions",
                newName: "IX_UserSubscriptions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_usersubscriptions_SubscriptionPlanId",
                table: "UserSubscriptions",
                newName: "IX_UserSubscriptions_SubscriptionPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_users_FamilyId",
                table: "Users",
                newName: "IX_Users_FamilyId");

            migrationBuilder.RenameIndex(
                name: "IX_users_Email",
                table: "Users",
                newName: "IX_Users_Email");

            migrationBuilder.RenameIndex(
                name: "IX_userfoodpreferences_UserId",
                table: "UserFoodPreferences",
                newName: "IX_UserFoodPreferences_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_tasks_FamilyId",
                table: "Tasks",
                newName: "IX_Tasks_FamilyId");

            migrationBuilder.RenameIndex(
                name: "IX_tasks_CreatedByUserId",
                table: "Tasks",
                newName: "IX_Tasks_CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_tasks_AssignedToUserId",
                table: "Tasks",
                newName: "IX_Tasks_AssignedToUserId");

            migrationBuilder.RenameIndex(
                name: "IX_taskproofs_TaskId",
                table: "TaskProofs",
                newName: "IX_TaskProofs_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_taskproofs_ChildUserId",
                table: "TaskProofs",
                newName: "IX_TaskProofs_ChildUserId");

            migrationBuilder.RenameIndex(
                name: "IX_rewards_FamilyId",
                table: "Rewards",
                newName: "IX_Rewards_FamilyId");

            migrationBuilder.RenameIndex(
                name: "IX_rewards_CreatedByUserId",
                table: "Rewards",
                newName: "IX_Rewards_CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_rewardredemptions_RewardId",
                table: "RewardRedemptions",
                newName: "IX_RewardRedemptions_RewardId");

            migrationBuilder.RenameIndex(
                name: "IX_rewardredemptions_ChildUserId",
                table: "RewardRedemptions",
                newName: "IX_RewardRedemptions_ChildUserId");

            migrationBuilder.RenameIndex(
                name: "IX_paymenttransactions_UserId",
                table: "PaymentTransactions",
                newName: "IX_PaymentTransactions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_paymenttransactions_SubscriptionPlanId",
                table: "PaymentTransactions",
                newName: "IX_PaymentTransactions_SubscriptionPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_notifications_UserId",
                table: "Notifications",
                newName: "IX_Notifications_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_mealsuggestions_RequestedByUserId",
                table: "MealSuggestions",
                newName: "IX_MealSuggestions_RequestedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_mealsuggestions_FamilyId",
                table: "MealSuggestions",
                newName: "IX_MealSuggestions_FamilyId");

            migrationBuilder.RenameIndex(
                name: "IX_familyevents_FamilyId",
                table: "FamilyEvents",
                newName: "IX_FamilyEvents_FamilyId");

            migrationBuilder.RenameIndex(
                name: "IX_familyevents_CreatedByUserId",
                table: "FamilyEvents",
                newName: "IX_FamilyEvents_CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_families_InviteCode",
                table: "Families",
                newName: "IX_Families_InviteCode");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSubscriptions",
                table: "UserSubscriptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFoodPreferences",
                table: "UserFoodPreferences",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskProofs",
                table: "TaskProofs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubscriptionPlans",
                table: "SubscriptionPlans",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Rewards",
                table: "Rewards",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RewardRedemptions",
                table: "RewardRedemptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentTransactions",
                table: "PaymentTransactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MealSuggestions",
                table: "MealSuggestions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FamilyEvents",
                table: "FamilyEvents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Families",
                table: "Families",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyEvents_Families_FamilyId",
                table: "FamilyEvents",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyEvents_Users_CreatedByUserId",
                table: "FamilyEvents",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MealSuggestions_Families_FamilyId",
                table: "MealSuggestions",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MealSuggestions_Users_RequestedByUserId",
                table: "MealSuggestions",
                column: "RequestedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_SubscriptionPlans_SubscriptionPlanId",
                table: "PaymentTransactions",
                column: "SubscriptionPlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_Users_UserId",
                table: "PaymentTransactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RewardRedemptions_Rewards_RewardId",
                table: "RewardRedemptions",
                column: "RewardId",
                principalTable: "Rewards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RewardRedemptions_Users_ChildUserId",
                table: "RewardRedemptions",
                column: "ChildUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rewards_Families_FamilyId",
                table: "Rewards",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rewards_Users_CreatedByUserId",
                table: "Rewards",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskProofs_Tasks_TaskId",
                table: "TaskProofs",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskProofs_Users_ChildUserId",
                table: "TaskProofs",
                column: "ChildUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Families_FamilyId",
                table: "Tasks",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_AssignedToUserId",
                table: "Tasks",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_CreatedByUserId",
                table: "Tasks",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFoodPreferences_Users_UserId",
                table: "UserFoodPreferences",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Families_FamilyId",
                table: "Users",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptions_SubscriptionPlans_SubscriptionPlanId",
                table: "UserSubscriptions",
                column: "SubscriptionPlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptions_Users_UserId",
                table: "UserSubscriptions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
