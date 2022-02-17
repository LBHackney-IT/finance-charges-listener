variable "environment_name" {
    type = string
    default = "staging"
}

variable "project_name" {
    type = string

    ### TODO - Update with the require project name. E.g. "Housing-Staging"
    default = "Housing-Development"
}
