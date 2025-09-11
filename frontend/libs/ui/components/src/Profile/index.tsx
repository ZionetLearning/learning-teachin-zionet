import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Typography, TextField, Stack } from "@mui/material";
import { useUpdateUserByUserId, User } from "@app-providers";
import { Button } from "../Button";
import { useStyles } from "./style";

export const Profile = ({ user }: { user: User }) => {
  const { t, i18n } = useTranslation();
  const isRTL = i18n.dir() === "rtl";
  const classes = useStyles();

  const { mutateAsync: updateUserMutation } = useUpdateUserByUserId(
    user?.userId ?? "",
  );

  const [userDetails, setUserDetails] = useState({
    firstName: user?.firstName ?? "",
    lastName: user?.lastName ?? "",
  });

  useEffect(() => {
    setUserDetails({
      firstName: user?.firstName ?? "",
      lastName: user?.lastName ?? "",
    });
  }, [user?.firstName, user?.lastName, user?.userId]);

  const handleChange =
    (field: "firstName" | "lastName") =>
    (e: React.ChangeEvent<HTMLInputElement>) => {
      setUserDetails((prev) => ({
        ...prev,
        [field]: e.target.value,
      }));
    };

  const handleCancel = () => {
    setUserDetails({
      firstName: user?.firstName ?? "",
      lastName: user?.lastName ?? "",
    });
  };

  const handleSave = async () => {
    await updateUserMutation({
      email: user.email,
      firstName: userDetails.firstName.trim(),
      lastName: userDetails.lastName.trim(),
    });
  };

  const dirty =
    userDetails.firstName.trim() !== (user?.firstName ?? "").trim() ||
    userDetails.lastName.trim() !== (user?.lastName ?? "").trim();

  return (
    <div className={classes.container}>
      <div className={classes.titleContainer}>
        <Typography variant="h4" className={classes.title}>
          {t("pages.profile.title")}
        </Typography>
      </div>

      <div className={classes.formCard}>
        <div className={classes.formHeader}>
          <Typography variant="h6">{t("pages.profile.subTitle")}</Typography>
          <Typography variant="body2" color="text.secondary">
            {t("pages.profile.secondSubTitle")}
          </Typography>
        </div>

        <Stack spacing={3}>
          <div className={classes.fieldContainer}>
            <Typography
              variant="body2"
              color="text.primary"
              className={isRTL ? classes.fieldLabelRTL : classes.fieldLabelLTR}
            >
              {t("pages.profile.firstName")}
            </Typography>
            <TextField
              value={userDetails.firstName}
              onChange={handleChange("firstName")}
              fullWidth
              className={isRTL ? classes.textFieldRTL : classes.textFieldLTR}
            />
          </div>

          <div className={classes.fieldContainer}>
            <Typography
              variant="body2"
              color="text.primary"
              className={isRTL ? classes.fieldLabelRTL : classes.fieldLabelLTR}
            >
              {t("pages.profile.lastName")}
            </Typography>
            <TextField
              value={userDetails.lastName}
              onChange={handleChange("lastName")}
              fullWidth
              className={isRTL ? classes.textFieldRTL : classes.textFieldLTR}
            />
          </div>

          <div className={classes.fieldContainer}>
            <Typography
              variant="body2"
              color="text.primary"
              className={
                isRTL ? classes.emailFieldLabelRTL : classes.emailFieldLabelLTR
              }
            >
              {t("pages.profile.email")}
            </Typography>
            <TextField
              value={user?.email}
              disabled
              fullWidth
              className={isRTL ? classes.textFieldRTL : classes.textFieldLTR}
            />
            <Typography
              variant="body2"
              color="text.disabled"
              className={
                isRTL
                  ? classes.emailDisabledNoteRTL
                  : classes.emailDisabledNoteLTR
              }
            >
              {t("pages.profile.emailCannotBeChanged")}
            </Typography>
          </div>
        </Stack>

        <div className={classes.buttonContainer}>
          <Button onClick={handleSave} disabled={!dirty}>
            {t("pages.profile.saveChanges")}
          </Button>
          <Button variant="outlined" disabled={!dirty} onClick={handleCancel}>
            {t("pages.profile.cancel")}
          </Button>
        </div>
      </div>
    </div>
  );
};
