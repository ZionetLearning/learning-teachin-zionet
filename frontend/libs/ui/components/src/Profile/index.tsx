import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Typography, TextField, Stack } from "@mui/material";
import { useUpdateUserByUserId, toAppRole } from "@app-providers";
import {
  User,
  HebrewLevelValue,
  PreferredLanguageCode,
} from "@app-providers/types";
import { useStyles } from "./style";
import { Dropdown, Button } from "@ui-components";

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
    hebrewLevelValue: user?.hebrewLevelValue ?? "beginner",
    preferredLanguageCode: user?.preferredLanguageCode ?? "en",
  });

  useEffect(() => {
    setUserDetails({
      firstName: user?.firstName ?? "",
      lastName: user?.lastName ?? "",
      hebrewLevelValue: user?.hebrewLevelValue ?? "beginner",
      preferredLanguageCode: user?.preferredLanguageCode ?? "en",
    });
  }, [
    user?.firstName,
    user?.hebrewLevelValue,
    user?.lastName,
    user?.preferredLanguageCode,
    user.userId,
  ]);

  const handleTextChange =
    (field: "firstName" | "lastName") =>
    (e: React.ChangeEvent<HTMLInputElement>) => {
      setUserDetails((prev) => ({
        ...prev,
        [field]: e.target.value,
      }));
    };

  const handleDropdownChange = (field: "hebrewLevelValue") => (val: string) => {
    setUserDetails((prev) => ({
      ...prev,
      [field]: val as HebrewLevelValue,
    }));
  };

  const handleLanguageChange = (val: string) => {
    setUserDetails((prev) => ({
      ...prev,
      preferredLanguageCode: val as PreferredLanguageCode,
    }));
  };

  const handleCancel = () => {
    setUserDetails({
      firstName: user?.firstName ?? "",
      lastName: user?.lastName ?? "",
      hebrewLevelValue: user?.hebrewLevelValue ?? "beginner",
      preferredLanguageCode: user?.preferredLanguageCode ?? "en",
    });
  };

  const handleSave = async () => {
    await updateUserMutation({
      email: user.email,
      firstName: userDetails.firstName.trim(),
      lastName: userDetails.lastName.trim(),
      hebrewLevelValue: userDetails.hebrewLevelValue,
      preferredLanguageCode: userDetails.preferredLanguageCode,
    });
  };

  const dirty =
    userDetails.firstName.trim() !== (user?.firstName ?? "").trim() ||
    userDetails.lastName.trim() !== (user?.lastName ?? "").trim() ||
    userDetails.hebrewLevelValue !== (user?.hebrewLevelValue ?? "beginner") ||
    userDetails.preferredLanguageCode !== (user?.preferredLanguageCode ?? "en");

  const hebrewLevelOptions = [
    { value: "beginner", label: t("hebrewLevels.beginner") },
    { value: "intermediate", label: t("hebrewLevels.intermediate") },
    { value: "advanced", label: t("hebrewLevels.advanced") },
    { value: "fluent", label: t("hebrewLevels.fluent") },
  ];

  const languageOptions = [
    { value: "he", label: t("languages.hebrew") },
    { value: "en", label: t("languages.english") },
  ];

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
              onChange={handleTextChange("firstName")}
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
              onChange={handleTextChange("lastName")}
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
              {/* Conditionally render Hebrew Level for student role */}
              {toAppRole(user?.role) === "student" && (
                <div className={classes.fieldContainer}>
                  <Typography
                    variant="body2"
                    color="text.primary"
                    className={
                      isRTL ? classes.fieldLabelRTL : classes.fieldLabelLTR
                    }
                  >
                    {t("hebrewLevels.title")}
                  </Typography>
                  <Dropdown
                    name="hebrewLevel"
                    options={hebrewLevelOptions}
                    value={userDetails.hebrewLevelValue}
                    onChange={(val) =>
                      handleDropdownChange("hebrewLevelValue")(val)
                    }
                  />
                </div>
              )}

              <div className={classes.fieldContainer}>
                <Typography
                  variant="body2"
                  color="text.primary"
                  className={
                    isRTL ? classes.fieldLabelRTL : classes.fieldLabelLTR
                  }
                >
                  {t("pages.profile.preferredLanguage")}
                </Typography>
                <Dropdown
                  name="preferredLanguage"
                  options={languageOptions}
                  value={userDetails.preferredLanguageCode}
                  onChange={handleLanguageChange}
                />
              </div>

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
