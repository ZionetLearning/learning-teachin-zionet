import { useState } from "react";

import { useTranslation } from "react-i18next";
import { toast } from "react-toastify";

import {
  getUserByUserId,
  useDeleteUserByUserId,
  useUpdateUserByUserId,
} from "@/api";
import { useStyles } from "./style";

export const UserListItem = ({
  userId,
  email,
}: {
  userId: string;
  email: string;
}) => {
  const { t } = useTranslation();
  const classes = useStyles();
  const { mutate: deleteUser, isPending } = useDeleteUserByUserId(userId);
  const { mutate: updateUser, isPending: isUpdating } =
    useUpdateUserByUserId(userId);

  const [editing, setEditing] = useState(false);
  const [emailValue, setEmailValue] = useState(email);
  const [passwordValue, setPasswordValue] = useState("");

  const handleDelete = () => {
    if (!confirm(t("pages.users.sureDelete", { email }))) return;
    deleteUser(undefined, {
      onSuccess: () => toast.success(t("pages.users.userDeleted")),
      onError: (e) => toast.error(e.message || t("pages.users.failedToDelete")),
    });
  };

  const beginEdit = () => {
    setEmailValue(email);
    setPasswordValue("");
    setEditing(true);
  };

  const cancelEdit = () => {
    setEditing(false);
    setEmailValue(email);
    setPasswordValue("");
  };

  const saveEdit = async () => {
    const trimmedEmail = emailValue.trim();
    const trimmedPassword = passwordValue.trim();
    if (!trimmedEmail) {
      toast.error(t("pages.users.emailRequired"));
      return;
    }
    if (trimmedPassword) {
      updateUser(
        { email: trimmedEmail, passwordHash: trimmedPassword },
        {
          onSuccess: () => {
            toast.success(t("pages.users.userUpdated"));
            setEditing(false);
          },
          onError: (e) =>
            toast.error(e.message || t("pages.users.failedToUpdate")),
        },
      );
      return;
    }
    if (trimmedEmail !== email) {
      try {
        const existing = await getUserByUserId(userId);
        if (!existing.passwordHash) {
          toast.error(t("pages.users.passwordMissingBackend"));
          return;
        }
        updateUser(
          { email: trimmedEmail, passwordHash: existing.passwordHash },
          {
            onSuccess: () => {
              toast.success(t("pages.users.userUpdated"));
              setEditing(false);
            },
            onError: (e) =>
              toast.error(e.message || t("pages.users.failedToUpdate")),
          },
        );
      } catch (err: unknown) {
        const message =
          err && typeof err === "object" && "message" in err
            ? (err as { message?: string }).message
            : undefined;
        toast.error(message || t("pages.users.failedToFetchUser"));
      }
      return;
    }
    toast.info(t("pages.users.nothingToUpdate"));
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
            placeholder={t("pages.users.emailPlaceholderEdit", {
              defaultValue: "email (leave blank to keep)",
            })}
            type="email"
            autoComplete="email"
            data-testid="users-edit-email"
          />
          <input
            className={classes.editInput}
            value={passwordValue}
            onChange={(e) => setPasswordValue(e.target.value)}
            placeholder={t("pages.users.newPasswordPlaceholder", {
              defaultValue: "new password (optional)",
            })}
            type="password"
            autoComplete="new-password"
            data-testid="users-edit-password"
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
            <span title={email} data-testid="users-email" data-cy="users-email">
              {email}
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
