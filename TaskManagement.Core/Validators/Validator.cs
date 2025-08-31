using FluentValidation;
using TaskManagement.Core.Entities;

namespace TaskManagement.Core.Validators
{
    public class TaskValidator : AbstractValidator<TaskItem>
    {
        public TaskValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

            RuleFor(x => x.ProjectId)
                .GreaterThan(0).WithMessage("Project is required");

            RuleFor(x => x.AssignedToId)
                .NotEmpty().WithMessage("Assignee is required");

            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(DateTime.Today).When(x => x.DueDate.HasValue)
                .WithMessage("Due date cannot be in the past");
        }
    }
}