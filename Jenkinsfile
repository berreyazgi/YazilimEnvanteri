pipeline {

    agent any

    options {
        timestamps()
        disableConcurrentBuilds()
    }

    stages {

        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Restore') {
            steps {
                sh '''
                    dotnet restore YazilimEnvanteri.slnx
                '''
            }
        }

        stage('Build') {
            steps {
                sh '''
                    dotnet build \
                    YazilimEnvanteri.slnx \
                    --configuration Release \
                    --no-restore
                '''
            }
        }

        stage('Test') {
            steps {
                sh '''
                    dotnet test \
                    YazilimEnvanteri.slnx \
                    --configuration Release \
                    --no-build
                '''
            }
        }

    }

    post {

        success {
            echo 'CI success.'
        }

        failure {
            echo 'CI failed.'
        }

    }
}