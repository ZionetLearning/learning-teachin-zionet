import { useState } from "react";

import { useTranslation } from "react-i18next";
import { toast } from "react-toastify";

import { useDeleteUserByUserId, useUpdateUserByUserId } from "@student/api";
import { useStyles } from "./style";

interface UserListItemProps {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
}

export const UserListItem = ({
  userId,
  email,
  firstName,
  lastName,
}: UserListItemProps) => {
  const { t } = useTranslation();
  const classes = useStyles();
  const { mutate: deleteUser, isPending } = useDeleteUserByUserId(userId);
  const { mutate: updateUser, isPending: isUpdating } =
    useUpdateUserByUserId(userId);

  const [editing, setEditing] = useState(false);
  const [emailValue, setEmailValue] = useState(email);
  const [firstNameValue, setFirstNameValue] = useState("");
  const [lastNameValue, setLastNameValue] = useState("");

  const handleDelete = () => {
    if (!confirm(t("pages.users.sureDelete", { email }))) return;
    deleteUser(undefined, {
      onSuccess: () => toast.success(t("pages.users.userDeleted")),
      onError: (e) => toast.error(e.message || t("pages.users.failedToDelete")),
    });
  };

  const beginEdit = () => {
    setEmailValue(email);
    setEditing(true);
    setFirstNameValue(firstName);
    setLastNameValue(lastName);
  };

  const cancelEdit = () => {
    setEditing(false);
    setEmailValue(email);
    setFirstNameValue(firstName);
    setLastNameValue(lastName);
  };

  const saveEdit = () => {
    if (!emailValue.trim()) {
      toast.error(t("pages.users.emailRequired"));
      return;
    }

    const payload: Record<string, string> = {};
    if (emailValue.trim() !== email) payload.email = emailValue.trim();
    if (firstNameValue.trim() && firstNameValue.trim() !== firstName)
      payload.firstName = firstNameValue.trim();
    if (lastNameValue.trim() && lastNameValue.trim() !== lastName)
      payload.lastName = lastNameValue.trim();

    if (Object.keys(payload).length === 0) {
      toast.info(t("pages.users.noChanges"));
      setEditing(false);
      return;
    }

    updateUser(payload, {
      onSuccess: () => {
        toast.success(t("pages.users.userUpdated"));
        setEditing(false);
        setFirstNameValue("");
        setLastNameValue("");
      },
      onError: (e) => toast.error(e.message || t("pages.users.failedToUpdate")),
    });
  };

  const initial = email?.charAt(0)?.toUpperCase() || "?";
  return (
    <li className={classes.listItem} data-testid={`users-item-${userId}`}>
      <div className={classes.avatar}>{initial}</div>
      {editing ? (
        <form
          className={classes.editForm}
          onSubmit={(e) => {
            e.preventDefault();
            saveEdit();
          }}
        >
          <input
            className={classes.editInput}
            value={emailValue}
            onChange={(e) => setEmailValue(e.target.value)}
            placeholder="email"
            type="email"
            autoComplete="email"
            data-testid="users-edit-email"
          />
          <input
            className={classes.editInput}
            value={firstNameValue}
            onChange={(e) => setFirstNameValue(e.target.value)}
            placeholder="first name"
            type="text"
            autoComplete="given-name"
            data-testid="users-edit-first-name"
          />
          <input
            className={classes.editInput}
            value={lastNameValue}
            onChange={(e) => setLastNameValue(e.target.value)}
            placeholder="last name"
            type="text"
            autoComplete="family-name"
            data-testid="users-edit-last-name"
          />
          <div className={classes.editActions}>
            <button
              className={classes.saveButton}
              type="submit"
              disabled={isUpdating}
              data-testid="users-edit-save"
            >
              {isUpdating ? t("pages.users.saving") : t("pages.users.save")}
            </button>
            <button
              type="button"
              onClick={cancelEdit}
              className={classes.cancelButton}
              disabled={isUpdating}
              data-testid="users-edit-cancel"
            >
              {t("pages.users.cancel")}
            </button>
          </div>
        </form>
      ) : (
        <>
          <div className={classes.info}>
            <span title={email} data-testid="users-email">
              {email}
            </span>
            <span title={firstName} data-testid="users-first-name">
              {firstName}
            </span>
            <span title={lastName} data-testid="users-last-name">
              {lastName}
            </span>
          </div>
          <div className={classes.actions}>
            <button
              onClick={beginEdit}
              className={classes.updateButton}
              disabled={isPending}
              data-testid="users-update-btn"
            >
              {isPending ? "..." : t("pages.users.update")}
            </button>
            <button
              onClick={handleDelete}
              className={classes.deleteButton}
              disabled={isPending}
              data-testid="users-delete-btn"
            >
              {isPending ? "..." : t("pages.users.delete")}
            </button>
          </div>
        </>
      )}
    </li>
  );
};
