import { useState } from "react";

import CloseIcon from "@mui/icons-material/Close";
import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import SaveIcon from "@mui/icons-material/Save";
import {
  IconButton,
  TableCell,
  TableRow,
  TextField,
  Tooltip,
} from "@mui/material";
import { useTranslation } from "react-i18next";
import { toast } from "react-toastify";

import { useDeleteUserByUserId, useUpdateUserByUserId } from "@admin/api";
import { AppRole, AppRoleType, User } from "@app-providers";
import { Dropdown } from "@ui-components";
import { useStyles } from "./style";
import { RoleBadge } from "./components";

interface UserListItemProps {
  user: User;
}

export const UserListItem = ({
  user: { userId, email, firstName, lastName, role },
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
  const [roleValue, setRoleValue] = useState<AppRoleType>(role);

  const handleDelete = () => {
    if (!confirm(t("pages.users.sureDelete", { email }))) return;
    deleteUser(undefined, {
      onSuccess: () => toast.success(t("pages.users.userDeleted")),
      onError: (e) => toast.error(e.message || t("pages.users.failedToDelete")),
    });
  };

  const beginEdit = () => {
    setEmailValue(email);
    setFirstNameValue(firstName);
    setLastNameValue(lastName);
    setRoleValue(role);
    setEditing(true);
  };

  const cancelEdit = () => {
    setEditing(false);
    setEmailValue(email);
    setFirstNameValue(firstName);
    setLastNameValue(lastName);
    setRoleValue(role);
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
    if (roleValue && roleValue !== role) payload.role = roleValue;

    if (Object.keys(payload).length === 0) {
      toast.info(t("pages.users.noChanges"));
      setEditing(false);
      return;
    }

    updateUser(payload, {
      onSuccess: () => {
        toast.success(t("pages.users.userUpdated"));
        setEditing(false);
      },
      onError: (e) => toast.error(e.message || t("pages.users.failedToUpdate")),
    });
  };

  return (
    <TableRow
      className={classes.tableRow}
      hover
      data-testid={`users-row-${userId}`}
    >
      <TableCell className={classes.tableCell} width="28%">
        {editing ? (
          <TextField
            size="small"
            type="email"
            value={emailValue}
            onChange={(e) => setEmailValue(e.target.value)}
            slotProps={{
              htmlInput: { "data-testid": "users-edit-email" },
            }}
            className={classes.textField}
            fullWidth
          />
        ) : (
          <span title={email} data-testid="users-email">
            {email}
          </span>
        )}
      </TableCell>
      <TableCell className={classes.tableCell}>
        {editing ? (
          <TextField
            size="small"
            value={firstNameValue}
            onChange={(e) => setFirstNameValue(e.target.value)}
            slotProps={{
              htmlInput: { "data-testid": "users-edit-first-name" },
            }}
            className={classes.textField}
            fullWidth
          />
        ) : (
          <span title={firstName} data-testid="users-first-name">
            {firstName}
          </span>
        )}
      </TableCell>
      <TableCell className={classes.tableCell}>
        {editing ? (
          <TextField
            size="small"
            value={lastNameValue}
            onChange={(e) => setLastNameValue(e.target.value)}
            slotProps={{
              htmlInput: { "data-testid": "users-edit-last-name" },
            }}
            className={classes.textField}
            fullWidth
          />
        ) : (
          <span title={lastName} data-testid="users-last-name">
            {lastName}
          </span>
        )}
      </TableCell>
      <TableCell
        className={`${classes.tableCell} ${classes.dropdownCell}`}
        width="16%"
      >
        {editing ? (
          <Dropdown
            name="role"
            label={t("pages.users.role")}
            options={(Object.values(AppRole) as AppRoleType[]).map((r) => ({
              value: r,
              label: t(`roles.${r}`),
            }))}
            value={roleValue}
            onChange={(val) => setRoleValue(val as AppRoleType)}
            disabled={isUpdating}
            data-testid="users-edit-role"
          />
        ) : (
          <RoleBadge
            role={role as AppRoleType}
            data-testid="users-role-badge"
          />
        )}
      </TableCell>
      <TableCell className={classes.tableCell} align="center" width="20%">
        {editing ? (
          <div className={classes.editActions}>
            <Tooltip
              title={
                isUpdating ? t("pages.users.saving") : t("pages.users.save")
              }
            >
              <span>
                <IconButton
                  className={classes.saveButton}
                  onClick={saveEdit}
                  disabled={isUpdating}
                  data-testid="users-edit-save"
                  aria-label={t("pages.users.save")}
                >
                  <SaveIcon />
                </IconButton>
              </span>
            </Tooltip>
            <Tooltip title={t("pages.users.cancel")}>
              <IconButton
                className={classes.cancelButton}
                onClick={cancelEdit}
                disabled={isUpdating}
                data-testid="users-edit-cancel"
                aria-label={t("pages.users.cancel")}
              >
                <CloseIcon />
              </IconButton>
            </Tooltip>
          </div>
        ) : (
          <div className={classes.actions}>
            <Tooltip title={t("pages.users.update")}>
              <span>
                <IconButton
                  className={classes.updateButton}
                  onClick={beginEdit}
                  disabled={isPending}
                  data-testid="users-update-btn"
                  aria-label={t("pages.users.update")}
                >
                  <EditIcon />
                </IconButton>
              </span>
            </Tooltip>
            <Tooltip title={t("pages.users.delete")}>
              <span>
                <IconButton
                  className={classes.deleteButton}
                  onClick={handleDelete}
                  disabled={isPending}
                  data-testid="users-delete-btn"
                  aria-label={t("pages.users.delete")}
                >
                  <DeleteIcon />
                </IconButton>
              </span>
            </Tooltip>
          </div>
        )}
      </TableCell>
    </TableRow>
  );
};
