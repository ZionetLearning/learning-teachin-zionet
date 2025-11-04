import { useRef, useState, ChangeEvent } from "react";
import { useTranslation } from "react-i18next";
import { Box, Avatar, IconButton, CircularProgress } from "@mui/material";
import PhotoCameraIcon from "@mui/icons-material/PhotoCamera";
import DeleteIcon from "@mui/icons-material/Delete";
import axios, { AxiosError } from "axios";
import { toast } from "react-toastify";
import {
  useGetAvatarUploadUrl,
  useConfirmAvatar,
  useGetAvatarUrl,
  useDeleteAvatar,
} from "@api";
import { useStyles } from "./style";

type AvatarUploadProps = {
  userId: string;
  userName: string;
};

const MAX_FILE_SIZE = 1048576; // 1MB
const ACCEPTED_TYPES = ["image/png", "image/jpeg"];
const ACCEPTED_EXTENSIONS = [".png", ".jpg", ".jpeg"];

export const AvatarUpload = ({ userId, userName }: AvatarUploadProps) => {
  const classes = useStyles();
  const { t } = useTranslation();
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [isUploading, setIsUploading] = useState(false);

  const { data: avatarUrl, isLoading: isLoadingUrl } = useGetAvatarUrl(userId);
  const { mutateAsync: getUploadUrl } = useGetAvatarUploadUrl(userId);
  const { mutateAsync: confirmAvatar } = useConfirmAvatar(userId);
  const { mutateAsync: deleteAvatar, isPending: isDeleting } =
    useDeleteAvatar(userId);

  const handleFileSelect = () => {
    fileInputRef.current?.click();
  };

  const validateFile = (file: File): string | null => {
    if (!ACCEPTED_TYPES.includes(file.type)) {
      const extension = file.name.split(".").pop()?.toLowerCase();
      if (!extension || !ACCEPTED_EXTENSIONS.includes(`.${extension}`)) {
        return t("pages.profile.avatar.invalidType");
      }
    }

    if (file.size > MAX_FILE_SIZE) {
      return t("pages.profile.avatar.fileTooLarge");
    }

    if (!file.type.startsWith("image/")) {
      return t("pages.profile.avatar.invalidType");
    }

    return null;
  };

  const handleFileChange = async (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];

    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }

    if (!file) return;

    const validationError = validateFile(file);
    if (validationError) {
      toast.error(validationError);
      return;
    }

    setIsUploading(true);

    try {
      const uploadData = await getUploadUrl({
        contentType: file.type,
        sizeBytes: file.size,
      });

      if (!uploadData?.uploadUrl || !uploadData?.blobPath) {
        throw new Error("Invalid upload URL received from server");
      }

      const uploadResponse = await axios.put(uploadData.uploadUrl, file, {
        headers: {
          "Content-Type": file.type,
          "x-ms-blob-type": "BlockBlob",
        },
        timeout: 60000, // 60 second timeout
      });

      if (uploadResponse.status !== 201) {
        throw new Error(
          `Upload failed with status ${uploadResponse.status}: ${uploadResponse.statusText}`,
        );
      }

      await confirmAvatar({
        blobPath: uploadData.blobPath,
        contentType: file.type,
      });
    } catch (error) {
      console.error("Avatar upload failed:", error);

      if (axios.isAxiosError(error)) {
        const axiosError = error as AxiosError;
        if (axiosError.code === "ECONNABORTED") {
          toast.error(t("pages.profile.avatar.uploadTimeout"));
        } else if (axiosError.response?.status === 413) {
          toast.error(t("pages.profile.avatar.fileTooLarge"));
        } else if (axiosError.response?.status === 403) {
          toast.error(t("pages.profile.avatar.uploadForbidden"));
        } else if (!navigator.onLine) {
          toast.error(t("pages.profile.avatar.noConnection"));
        } else {
          toast.error(
            t("pages.profile.avatar.uploadFailed", {
              error: axiosError.message,
            }),
          );
        }
      } else if (error instanceof Error) {
        toast.error(
          t("pages.profile.avatar.uploadFailed", { error: error.message }),
        );
      } else {
        toast.error(t("pages.profile.avatar.uploadFailedGeneric"));
      }
    } finally {
      setIsUploading(false);
    }
  };

  const handleDelete = async () => {
    if (window.confirm(t("pages.profile.avatar.confirmDelete"))) {
      await deleteAvatar();
    }
  };

  const getInitials = () => {
    const names = userName.split(" ");
    if (names.length >= 2) {
      return `${names[0][0]}${names[1][0]}`.toUpperCase();
    }
    return userName.substring(0, 2).toUpperCase();
  };

  const isLoading = isLoadingUrl || isUploading || isDeleting;

  return (
    <Box className={classes.container}>
      <Box className={classes.avatarWrapper}>
        <Avatar
          src={avatarUrl || undefined}
          alt={userName}
          className={classes.avatar}
        >
          {!avatarUrl && getInitials()}
        </Avatar>
        {isLoading && (
          <Box className={classes.loadingOverlay}>
            <CircularProgress size={40} />
          </Box>
        )}
      </Box>

      <Box className={classes.buttonGroup}>
        <IconButton
          onClick={handleFileSelect}
          disabled={isLoading}
          className={classes.iconButton}
          title={t("pages.profile.avatar.upload")}
        >
          <PhotoCameraIcon />
        </IconButton>

        {avatarUrl && (
          <IconButton
            onClick={handleDelete}
            disabled={isLoading}
            className={classes.iconButton}
            title={t("pages.profile.avatar.delete")}
          >
            <DeleteIcon />
          </IconButton>
        )}
      </Box>

      <input
        ref={fileInputRef}
        type="file"
        accept={ACCEPTED_TYPES.join(",")}
        onChange={handleFileChange}
        style={{ display: "none" }}
      />
    </Box>
  );
};
